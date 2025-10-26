using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ENOC.Application.Configuration;
using ENOC.Application.DTOs.Auth;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ENOC.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtConfig _jwtConfig;
    private readonly AdConfig _adConfig;
    private readonly FeatureFlags _featureFlags;
    private readonly DevAccount _devAccount;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IOptions<JwtConfig> jwtConfig,
        IOptions<AdConfig> adConfig,
        IOptions<FeatureFlags> featureFlags,
        IOptions<DevAccount> devAccount)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _jwtConfig = jwtConfig.Value;
        _adConfig = adConfig.Value;
        _featureFlags = featureFlags.Value;
        _devAccount = devAccount.Value;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Authenticate based on feature flag
        if (_featureFlags.UseActiveDirectory)
        {
            // Validate against AD
            var isValidAdUser = await ValidateAdCredentialsAsync(request.Username, request.Password);
            if (!isValidAdUser)
            {
                return null;
            }
        }
        else
        {
            // For local development, allow any password for dev account
            if (!request.Username.Equals(_devAccount.Username, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }

        // Find or create user in database
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            // Get default team (Black team) for dev user
            Guid? defaultTeamId = null;
            if (!_featureFlags.UseActiveDirectory)
            {
                var blackTeam = (await _unitOfWork.Repository<Team>().GetAllAsync(cancellationToken))
                    .FirstOrDefault(t => t.Name == "Black");
                defaultTeamId = blackTeam?.Id;
            }

            // Create new user from AD or dev account
            user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Username,
                EmployeeId = _featureFlags.UseActiveDirectory ? request.Username : _devAccount.EmployeeId,
                FirstName = _featureFlags.UseActiveDirectory ? "Unknown" : _devAccount.FirstName,
                LastName = _featureFlags.UseActiveDirectory ? "Unknown" : _devAccount.LastName,
                TeamId = defaultTeamId,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return null;
            }

            // Assign Management role to dev user
            if (!_featureFlags.UseActiveDirectory)
            {
                await _userManager.AddToRoleAsync(user, "Management");
            }
        }

        // Check if user is active
        if (!user.IsActive || user.IsDeleted)
        {
            return null;
        }

        // Generate tokens
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token
        var tokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id.ToString(),
            Token = refreshToken,
            TokenExpiry = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenValidaty),
            Application = request.Application
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(tokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiry = DateTime.UtcNow.AddDays(_jwtConfig.TokenValidaty),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                EmployeeId = user.EmployeeId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                TeamId = user.TeamId,
                PositionId = user.PositionId,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenRepo = _unitOfWork.Repository<RefreshToken>();
        var storedToken = await tokenRepo.FirstOrDefaultAsync(
            t => t.Token == refreshToken && t.TokenExpiry > DateTime.UtcNow,
            cancellationToken);

        if (storedToken == null)
        {
            return null;
        }

        // Get user
        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive || user.IsDeleted)
        {
            return null;
        }

        // Generate new tokens
        var newAccessToken = await GenerateAccessTokenAsync(user);
        var newRefreshToken = GenerateRefreshToken();

        // Update refresh token
        storedToken.Token = newRefreshToken;
        storedToken.TokenExpiry = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenValidaty);
        tokenRepo.Update(storedToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenExpiry = DateTime.UtcNow.AddDays(_jwtConfig.TokenValidaty),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                EmployeeId = user.EmployeeId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                TeamId = user.TeamId,
                PositionId = user.PositionId,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<bool> LogoutAsync(Guid userId, string? application = null, CancellationToken cancellationToken = default)
    {
        var tokenRepo = _unitOfWork.Repository<RefreshToken>();
        var tokens = await tokenRepo.FindAsync(
            t => t.UserId == userId.ToString() && (application == null || t.Application == application),
            cancellationToken);

        if (tokens.Any())
        {
            tokenRepo.RemoveRange(tokens);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> ValidateAdCredentialsAsync(string username, string password)
    {
        if (!_featureFlags.UseActiveDirectory)
        {
            return false;
        }

        try
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                _adConfig.DominName,
                _adConfig.DomainIP);

            return context.ValidateCredentials(username, password);
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("EmployeeId", user.EmployeeId),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtConfig.TokenValidaty),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
