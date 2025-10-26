using ENOC.Domain.Entities;
using ENOC.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ENOC.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        // Ensure database is created
        await _context.Database.MigrateAsync();

        // Seed in order due to foreign key relationships
        await SeedRolesAsync();
        await SeedBusinessUnitsAsync();
        await SeedTeamsAsync();
        await SeedEercPositionsAsync();
        await SeedTestUserAsync(); // Add test user for development (matches FakeAuthMiddleware)
        await SeedIncidentTypesAsync();
        await SeedMessagesAsync();
        await SeedTanksAsync();
        await SeedVehiclesAsync();

        await _context.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Management", "EERC" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private async Task SeedBusinessUnitsAsync()
    {
        if (await _context.BusinessUnits.AnyAsync())
            return;

        var businessUnits = new[]
        {
            new BusinessUnit { Id = Guid.NewGuid(), Name = "EPCL" },
            new BusinessUnit { Id = Guid.NewGuid(), Name = "EIL" },
            new BusinessUnit { Id = Guid.NewGuid(), Name = "DOUGAS" },
            new BusinessUnit { Id = Guid.NewGuid(), Name = "EMGAS" },
            new BusinessUnit { Id = Guid.NewGuid(), Name = "HEJP - FALCON" },
            new BusinessUnit { Id = Guid.NewGuid(), Name = "HJATL" },
            new BusinessUnit { Id = Guid.NewGuid(), Name = "DLPP" }
        };

        await _context.BusinessUnits.AddRangeAsync(businessUnits);
    }

    private async Task SeedTeamsAsync()
    {
        if (await _context.Teams.AnyAsync())
            return;

        var teams = new[]
        {
            new Team { Id = Guid.NewGuid(), Name = "White", Color = "#FFFFFF", ColorName = "White" },
            new Team { Id = Guid.NewGuid(), Name = "Black", Color = "#000000", ColorName = "Black" },
            new Team { Id = Guid.NewGuid(), Name = "Green", Color = "#00FF00", ColorName = "Green" },
            new Team { Id = Guid.NewGuid(), Name = "Red", Color = "#FF0000", ColorName = "Red" }
        };

        await _context.Teams.AddRangeAsync(teams);
    }

    private async Task SeedEercPositionsAsync()
    {
        if (await _context.EercPositions.AnyAsync())
            return;

        var positions = new[]
        {
            new EercPosition { Id = Guid.NewGuid(), Name = "EERC Commander" },
            new EercPosition { Id = Guid.NewGuid(), Name = "Operations Officer" },
            new EercPosition { Id = Guid.NewGuid(), Name = "Safety Officer" },
            new EercPosition { Id = Guid.NewGuid(), Name = "Logistics Officer" },
            new EercPosition { Id = Guid.NewGuid(), Name = "Planning Officer" },
            new EercPosition { Id = Guid.NewGuid(), Name = "Communications Officer" },
            new EercPosition { Id = Guid.NewGuid(), Name = "Field Responder" }
        };

        await _context.EercPositions.AddRangeAsync(positions);
    }

    private async Task SeedTestUserAsync()
    {
        // Create test user with the exact ID that FakeAuthMiddleware uses
        var testUserId = new Guid("00000000-0000-0000-0000-000000000001");

        // Check if a user with this email already exists
        var existingUserByEmail = await _userManager.FindByEmailAsync("dev@enoc.local");
        if (existingUserByEmail != null)
        {
            // If user exists but has wrong ID, delete it first
            if (existingUserByEmail.Id != testUserId)
            {
                await _userManager.DeleteAsync(existingUserByEmail);
            }
            else
            {
                // User already has the correct ID, nothing to do
                return;
            }
        }

        // Check if test user with correct ID already exists
        var existingUserById = await _userManager.FindByIdAsync(testUserId.ToString());
        if (existingUserById != null)
            return;

        // Get a team and position for the test user
        var whiteTeam = await _context.Teams.FirstOrDefaultAsync(t => t.Name == "White");
        var commanderPosition = await _context.EercPositions.FirstOrDefaultAsync(p => p.Name == "EERC Commander");

        var testUser = new ApplicationUser
        {
            Id = testUserId,
            UserName = "dev@enoc.local",
            Email = "dev@enoc.local",
            NormalizedUserName = "DEV@ENOC.LOCAL",
            NormalizedEmail = "DEV@ENOC.LOCAL",
            EmailConfirmed = true,
            FirstName = "Dev",
            LastName = "User",
            EmployeeId = "DEV001",
            TeamId = whiteTeam?.Id,
            PositionId = commanderPosition?.Id,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(testUser);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        // Assign EERC role to the test user
        await _userManager.AddToRoleAsync(testUser, "EERC");
    }

    private async Task SeedIncidentTypesAsync()
    {
        if (await _context.IncidentTypes.AnyAsync())
            return;

        var incidentTypes = new[]
        {
            new IncidentType { Id = Guid.NewGuid(), Name = "Fire (full emergency)", Image = "/assets/alert-fire.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Weather standby", Image = "/assets/alert-smoke.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Unlawful act", Image = "/assets/alert-gas-leak.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Domestic fire", Image = "/assets/alert-chimical-leak.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Oil spillage", Image = "/assets/alert-dangerous-goods.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Fuel spillage", Image = "/assets/alert-dangerous-goods.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Dangerous goods", Image = "/assets/alert-dangerous-goods.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Chemical leak", Image = "/assets/alert-chimical-leak.png" },
            new IncidentType { Id = Guid.NewGuid(), Name = "Gas leak", Image = "/assets/alert-gas-leak.png" }
        };

        await _context.IncidentTypes.AddRangeAsync(incidentTypes);
    }

    private async Task SeedMessagesAsync()
    {
        if (await _context.Messages.AnyAsync())
            return;

        var messages = new[]
        {
            new Message { Id = Guid.NewGuid(), Description = "Emergency response team dispatched to location" },
            new Message { Id = Guid.NewGuid(), Description = "Evacuation procedures initiated" },
            new Message { Id = Guid.NewGuid(), Description = "All personnel accounted for" },
            new Message { Id = Guid.NewGuid(), Description = "Hazmat team en route" },
            new Message { Id = Guid.NewGuid(), Description = "Incident contained, monitoring situation" },
            new Message { Id = Guid.NewGuid(), Description = "Medical assistance required" },
            new Message { Id = Guid.NewGuid(), Description = "Fire suppression systems activated" },
            new Message { Id = Guid.NewGuid(), Description = "Securing the perimeter" },
            new Message { Id = Guid.NewGuid(), Description = "Conducting safety assessment" },
            new Message { Id = Guid.NewGuid(), Description = "All clear - resuming normal operations" }
        };

        await _context.Messages.AddRangeAsync(messages);
    }

    private async Task SeedTanksAsync()
    {
        if (await _context.Tanks.AnyAsync())
            return;

        var businessUnits = await _context.BusinessUnits.ToListAsync();
        if (!businessUnits.Any())
            return;

        var tanks = new List<Tank>();
        var random = new Random(42); // Fixed seed for consistent data

        // Create 20 tanks distributed across business units
        for (int i = 1; i <= 20; i++)
        {
            var businessUnit = businessUnits[i % businessUnits.Count];

            // Generate random coordinates around Dubai (25.2048° N, 55.2708° E)
            var latitude = 25.2048 + (random.NextDouble() - 0.5) * 0.1;
            var longitude = 55.2708 + (random.NextDouble() - 0.5) * 0.1;

            tanks.Add(new Tank
            {
                Id = Guid.NewGuid(),
                Name = $"Tank {i}",
                TankNumber = i,
                BusinessUnitId = businessUnit.Id,
                Location = $"{latitude:F6},{longitude:F6}",
                ERG = $"ERG-{100 + i}"
            });
        }

        await _context.Tanks.AddRangeAsync(tanks);
    }

    private async Task SeedVehiclesAsync()
    {
        if (await _context.Vehicles.AnyAsync())
            return;

        var vehicles = new[]
        {
            new Vehicle { Id = Guid.NewGuid(), Name = "Fire Truck 1", PlateNumber = "DXB-F001" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Fire Truck 2", PlateNumber = "DXB-F002" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Ambulance 1", PlateNumber = "DXB-A001" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Ambulance 2", PlateNumber = "DXB-A002" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Hazmat Unit 1", PlateNumber = "DXB-H001" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Command Vehicle", PlateNumber = "DXB-C001" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Water Tanker 1", PlateNumber = "DXB-W001" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Water Tanker 2", PlateNumber = "DXB-W002" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Foam Unit 1", PlateNumber = "DXB-FO001" },
            new Vehicle { Id = Guid.NewGuid(), Name = "Rescue Truck", PlateNumber = "DXB-R001" }
        };

        await _context.Vehicles.AddRangeAsync(vehicles);
    }
}
