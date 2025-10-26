using ENOC.Application.DTOs.User;

namespace ENOC.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
