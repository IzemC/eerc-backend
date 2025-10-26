namespace ENOC.Application.DTOs.User;

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? TeamId { get; set; }
    public Guid? PositionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
}
