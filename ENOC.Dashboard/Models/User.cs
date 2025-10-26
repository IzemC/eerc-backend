namespace ENOC.Dashboard.Models;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TeamName { get; set; }
    public Guid? TeamId { get; set; }
    public string? PositionName { get; set; }
    public Guid? PositionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsDeleted { get; set; }
}

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? TeamId { get; set; }
    public Guid? PositionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? TeamId { get; set; }
    public Guid? PositionId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
}
