using System.Text;
using ENOC.Application.Configuration;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using ENOC.Infrastructure.Data;
using ENOC.Infrastructure.Repositories;
using ENOC.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ENOC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration options
        services.Configure<JwtConfig>(configuration.GetSection("JwtConfig"));
        services.Configure<AdConfig>(configuration.GetSection("AdConfig"));
        services.Configure<FeatureFlags>(configuration.GetSection("FeatureFlags"));
        services.Configure<DevAccount>(configuration.GetSection("DevAccount"));
        services.Configure<EmailConfig>(configuration.GetSection("EmailConfig"));
        services.Configure<SmsConfig>(configuration.GetSection("SmsConfig"));
        services.Configure<FcmConfig>(configuration.GetSection("FcmConfig"));
        services.Configure<WeatherConfig>(configuration.GetSection("WeatherConfig"));

        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServerConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Identity - for user management only, authentication is via AD
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Disable password requirements since AD handles authentication
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 1;

            // User settings
            options.User.RequireUniqueEmail = false;

            // Sign-in settings - we'll handle this manually with AD
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtConfig = configuration.GetSection("JwtConfig").Get<JwtConfig>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig!.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Repository pattern
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IShiftReportService, ShiftReportService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<ICrewVehicleListingService, CrewVehicleListingService>();
        services.AddScoped<ITankService, TankService>();
        services.AddScoped<IDeviceTokenService, DeviceTokenService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IMapService, MapService>();

        // Notification services with feature flags
        var featureFlags = configuration.GetSection("FeatureFlags").Get<FeatureFlags>();

        if (featureFlags?.UseEmailService == true)
        {
            services.AddScoped<IEmailService, EmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, MockEmailService>();
        }

        if (featureFlags?.UseSmsService == true)
        {
            services.AddScoped<ISmsService, SmsService>();
        }
        else
        {
            services.AddScoped<ISmsService, MockSmsService>();
        }

        if (featureFlags?.UseFcmService == true)
        {
            services.AddScoped<IPushNotificationService, PushNotificationService>();
        }
        else
        {
            services.AddScoped<IPushNotificationService, MockPushNotificationService>();
        }

        if (featureFlags?.UseWeatherService == true)
        {
            services.AddScoped<IWeatherService, WeatherService>();
        }
        else
        {
            services.AddScoped<IWeatherService, MockWeatherService>();
        }

        // HttpClient for SMS, FCM, and Weather services
        services.AddHttpClient();

        // Note: INotificationService is registered in API layer to avoid circular dependency

        return services;
    }
}
