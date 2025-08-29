using ENOC.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ENOC.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.EmployeeId).IsUnique();
                entity.HasIndex(u => u.AdId).IsUnique();
                entity.HasIndex(u => u.IsActive);
                entity.HasIndex(u => u.IsDeleted);
            });
        }
    }
}