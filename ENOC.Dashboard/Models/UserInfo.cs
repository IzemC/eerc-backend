namespace ENOC.Dashboard.Models;

public class UserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Team { get; set; }
    public Guid? TeamId { get; set; }
    public string? Position { get; set; }
    public Guid? PositionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
