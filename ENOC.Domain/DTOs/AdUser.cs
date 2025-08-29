namespace ENOC.Domain.DTOs
{
    public class AdUser
    {
        public Guid? Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public List<string> Roles { get; set; }
        public required string EercPosition { get; set; }
        public required string EercTeam { get; set; }
        public required string PhoneNumber { get; set; }
        public required string EmployeeId { get; set; }
        
        public AdUser()
        {
            Roles = [];
        }
    }
}