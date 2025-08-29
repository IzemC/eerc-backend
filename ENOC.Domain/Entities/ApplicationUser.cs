using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ENOC.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(150)]
        public required string FirstName { get; set; }

        [StringLength(150)]
        public required string LastName { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Guid? TeamId { get; set; }

        public Guid? PositionId { get; set; }

        public required string EmployeeId { get; set; }

        public string? Signature { get; set; }

        public Guid? AdId { get; set; }
    }
}
