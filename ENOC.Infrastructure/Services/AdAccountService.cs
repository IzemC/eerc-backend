using ENOC.Domain.DTOs;
using ENOC.Domain.Models;
using ENOC.Domain.Extensions;
using ENOC.Domain.Entities;
using ENOC.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.DirectoryServices.AccountManagement;

namespace ENOC.Infrastructure.Services
{
    public class AdAccountService
    {
        private readonly AdConfig _adConfig;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdAccountService(
            IOptions<AdConfig> adConfig, 
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _adConfig = adConfig.Value;
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdUser?> ValidateUser(string username, string password)
        {
            using var adContext = new PrincipalContext(ContextType.Domain, _adConfig.DomainName);
            var result = adContext.ValidateCredentials(username, password);
            if (!result)
            {
                return null;
            }

            var user = UserPrincipal.FindByIdentity(adContext, IdentityType.SamAccountName, username);

            if (user?.Enabled != true)
            {
                return null;
            }

            var userGroups = user.GetGroups();
            var roles = new List<string>();
            var eercUserTeam = "";

            foreach (Principal p in userGroups)
            {
                if (p is GroupPrincipal group)
                {
                    if (group.Name == "_EERC Black Group")
                    {
                        eercUserTeam = "black";
                        roles.Add("_EERC");
                    }
                    else if (group.Name == "_EERC Red Group")
                    {
                        eercUserTeam = "red";
                        roles.Add("_EERC");
                    }
                    else if (group.Name == "_EERC White Group")
                    {
                        eercUserTeam = "white";
                        roles.Add("_EERC");
                    }
                    else if (group.Name == "_EERC Green Group")
                    {
                        eercUserTeam = "green";
                        roles.Add("_EERC");
                    }
                    else
                    {
                        roles.Add(group.Name);
                    }
                }
            }

            var eercUserPosition = user.GetJob();

            // Sync with local database
            await SyncUserWithDatabase(user, roles, eercUserPosition, eercUserTeam);

            return new AdUser
            {
                Id = user.Guid,
                UserName = username,
                FirstName = user.GivenName,
                LastName = user.Surname,
                Email = user.EmailAddress,
                PhoneNumber = user.VoiceTelephoneNumber,
                Roles = roles,
                EercPosition = eercUserPosition,
                EercTeam = eercUserTeam,
                EmployeeId = user.EmployeeId,
            };
        }

        private async Task SyncUserWithDatabase(UserPrincipal user, List<string> roles, string position, string team)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.AdId == user.Guid || u.UserName == user.SamAccountName);

            if (existingUser == null)
            {
                // Create new user
                var newUser = new ApplicationUser
                {
                    UserName = user.SamAccountName,
                    Email = user.EmailAddress,
                    FirstName = user.GivenName,
                    LastName = user.Surname,
                    PhoneNumber = user.VoiceTelephoneNumber,
                    EmployeeId = user.EmployeeId,
                    AdId = user.Guid,
                    IsActive = user.Enabled ?? true
                };

                var result = await _userManager.CreateAsync(newUser);
                if (result.Succeeded && roles.Any())
                {
                    await _userManager.AddToRolesAsync(newUser, roles);
                }
            }
            else
            {
                // Update existing user
                existingUser.FirstName = user.GivenName;
                existingUser.LastName = user.Surname;
                existingUser.Email = user.EmailAddress;
                existingUser.PhoneNumber = user.VoiceTelephoneNumber;
                existingUser.EmployeeId = user.EmployeeId;
                existingUser.AdId = user.Guid;
                existingUser.IsActive = user.Enabled ?? true;

                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();

                // Update user roles
                if (roles.Any())
                {
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);
                    await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                    await _userManager.AddToRolesAsync(existingUser, roles);
                }
            }
        }
    }
}