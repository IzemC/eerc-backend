namespace ENOC.Application.DTOs.User;

public class UserResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Team { get; set; }
    public Guid? TeamId { get; set; }
    public string? Position { get; set; }
    public Guid? PositionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
