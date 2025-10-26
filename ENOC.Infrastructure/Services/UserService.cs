using ENOC.Application.DTOs.User;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = _userManager.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();

        var teams = (await _unitOfWork.Repository<Team>().GetAllAsync(cancellationToken)).ToList();
        var positions = (await _unitOfWork.Repository<EercPosition>().GetAllAsync(cancellationToken)).ToList();

        var userResponses = new List<UserResponse>();

        foreach (var user in users)
        {
            var team = teams.FirstOrDefault(t => t.Id == user.TeamId);
            var position = positions.FirstOrDefault(p => p.Id == user.PositionId);
            var roles = await _userManager.GetRolesAsync(user);

            userResponses.Add(new UserResponse
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty,
                Team = team?.Name,
                TeamId = user.TeamId,
                Position = position?.Name,
                PositionId = user.PositionId,
                EmployeeId = user.EmployeeId,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return userResponses;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
        {
            return null;
        }

        var team = user.TeamId.HasValue
            ? await _unitOfWork.Repository<Team>().GetByIdAsync(user.TeamId.Value, cancellationToken)
            : null;

        var position = user.PositionId.HasValue
            ? await _unitOfWork.Repository<EercPosition>().GetByIdAsync(user.PositionId.Value, cancellationToken)
            : null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email ?? string.Empty,
            Team = team?.Name,
            TeamId = user.TeamId,
            Position = position?.Name,
            PositionId = user.PositionId,
            EmployeeId = user.EmployeeId,
            Roles = roles.ToList(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Split full name into first and last name
        var nameParts = request.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = firstName,
            LastName = lastName,
            EmployeeId = request.EmployeeId,
            TeamId = request.TeamId,
            PositionId = request.PositionId,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user: {Errors}", errors);
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        _logger.LogInformation("Created user {UserId} ({Email})", user.Id, user.Email);

        return (await GetUserByIdAsync(user.Id, cancellationToken))!;
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
        {
            return null;
        }

        // Update full name if provided
        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            var nameParts = request.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        }

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.Email = request.Email;
            user.UserName = request.Email;
        }

        // Update team if provided
        if (request.TeamId.HasValue)
        {
            user.TeamId = request.TeamId.Value;
        }

        // Update position if provided
        if (request.PositionId.HasValue)
        {
            user.PositionId = request.PositionId.Value;
        }

        // Update active status if provided
        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to update user {UserId}: {Errors}", userId, errors);
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        _logger.LogInformation("Updated user {UserId}", userId);

        return await GetUserByIdAsync(userId, cancellationToken);
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.IsDeleted)
        {
            return false;
        }

        // Soft delete
        user.IsDeleted = true;
        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to delete user {UserId}: {Errors}", userId, errors);
            return false;
        }

        _logger.LogInformation("Deleted user {UserId}", userId);

        return true;
    }
}
