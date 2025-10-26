using ENOC.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ENOC.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        // Core entities
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<BusinessUnit> BusinessUnits { get; set; }
        public DbSet<Tank> Tanks { get; set; }
        public DbSet<TankFile> TankFiles { get; set; }
        public DbSet<IncidentType> IncidentTypes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<IncidentAcknowledgement> IncidentAcknowledgements { get; set; }

        // Report/Form entities
        public DbSet<ShiftReportForm> ShiftReportForms { get; set; }
        public DbSet<CrewVehicleListingForm> CrewVehicleListingForms { get; set; }
        public DbSet<Inspection> Inspections { get; set; }

        // Status entry entities
        public DbSet<TeamStatusEntry> TeamStatusEntries { get; set; }
        public DbSet<FireVehicleStatusEntry> FireVehicleStatusEntries { get; set; }
        public DbSet<SCBAStatusEntry> SCBAStatusEntries { get; set; }
        public DbSet<RadioStatusEntry> RadioStatusEntries { get; set; }

        // Lookup/Reference entities
        public DbSet<Team> Teams { get; set; }
        public DbSet<EercPosition> EercPositions { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<SCBA> SCBAs { get; set; }
        public DbSet<Radio> Radios { get; set; }
        public DbSet<EmployeeStatus> EmployeeStatuses { get; set; }
        public DbSet<EercStatus> EercStatuses { get; set; }

        // Notification/Token entities
        public DbSet<FCMToken> FCMTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.EmployeeId).IsUnique();
                entity.HasIndex(u => u.AdId).IsUnique();
                entity.HasIndex(u => u.IsActive);
                entity.HasIndex(u => u.IsDeleted);
            });

            // Incident configuration
            builder.Entity<Incident>(entity =>
            {
                entity.HasIndex(i => i.IncidentId).IsUnique();
                entity.HasIndex(i => i.Status);
                entity.HasIndex(i => i.CreatedAt);
            });

            // Tank configuration
            builder.Entity<Tank>(entity =>
            {
                entity.HasIndex(t => t.TankNumber);
                entity.HasIndex(t => t.IsDeleted);
            });

            // Configure cascade delete behavior to prevent circular cascades
            // TeamStatusEntry relationships
            builder.Entity<TeamStatusEntry>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeamStatusEntry>()
                .HasOne(t => t.EmployeeStatus)
                .WithMany(e => e.TeamStatusEntries)
                .HasForeignKey(t => t.EmployeeStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeamStatusEntry>()
                .HasOne(t => t.Vehicle)
                .WithMany(v => v.TeamStatusEntries)
                .HasForeignKey(t => t.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // FireVehicleStatusEntry relationships
            builder.Entity<FireVehicleStatusEntry>()
                .HasOne(f => f.Vehicle)
                .WithMany(v => v.FireVehicleStatusEntries)
                .HasForeignKey(f => f.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // SCBAStatusEntry relationships
            builder.Entity<SCBAStatusEntry>()
                .HasOne(s => s.SCBA)
                .WithMany(scba => scba.SCBAStatusEntries)
                .HasForeignKey(s => s.SCBAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SCBAStatusEntry>()
                .HasOne(s => s.Vehicle)
                .WithMany(v => v.SCBAStatusEntries)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // RadioStatusEntry relationships
            builder.Entity<RadioStatusEntry>()
                .HasOne(r => r.Radio)
                .WithMany(radio => radio.RadioStatusEntries)
                .HasForeignKey(r => r.RadioId)
                .OnDelete(DeleteBehavior.Restrict);

            // CrewVehicleListingForm relationships
            builder.Entity<CrewVehicleListingForm>()
                .HasOne(c => c.Team)
                .WithMany(t => t.CrewVehicleListingForms)
                .HasForeignKey(c => c.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // ShiftReportForm relationships
            builder.Entity<ShiftReportForm>()
                .HasOne(s => s.Team)
                .WithMany(t => t.ShiftReportForms)
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // IncidentAcknowledgement relationships
            builder.Entity<IncidentAcknowledgement>()
                .HasOne(ia => ia.User)
                .WithMany()
                .HasForeignKey(ia => ia.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<IncidentAcknowledgement>()
                .HasOne(ia => ia.Incident)
                .WithMany(i => i.Acknowledgements)
                .HasForeignKey(ia => ia.IncidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Incident relationships
            builder.Entity<Incident>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Incident>()
                .HasOne(i => i.IncidentType)
                .WithMany(it => it.Incidents)
                .HasForeignKey(i => i.IncidentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Incident>()
                .HasOne(i => i.Unit)
                .WithMany(u => u.Incidents)
                .HasForeignKey(i => i.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Incident>()
                .HasOne(i => i.Message)
                .WithMany(m => m.Incidents)
                .HasForeignKey(i => i.MessageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Incident>()
                .HasOne(i => i.Tank)
                .WithMany(t => t.Incidents)
                .HasForeignKey(i => i.TankId)
                .OnDelete(DeleteBehavior.Restrict);

            // Inspection relationships
            builder.Entity<Inspection>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inspection>()
                .HasOne(i => i.Vehicle)
                .WithMany(v => v.Inspections)
                .HasForeignKey(i => i.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tank relationships
            builder.Entity<Tank>()
                .HasOne(t => t.BusinessUnit)
                .WithMany(u => u.Tanks)
                .HasForeignKey(t => t.BusinessUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // TankFile relationships
            builder.Entity<TankFile>()
                .HasOne(tf => tf.Tank)
                .WithMany(t => t.Files)
                .HasForeignKey(tf => tf.TankId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserDeviceToken configuration
            builder.Entity<UserDeviceToken>(entity =>
            {
                entity.HasIndex(t => t.DeviceToken);
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.IsActive);

                entity.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}