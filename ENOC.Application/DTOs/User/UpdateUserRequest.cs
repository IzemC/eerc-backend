namespace ENOC.Application.DTOs.User;

public class UpdateUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? PositionId { get; set; }
    public bool? IsActive { get; set; }
}
