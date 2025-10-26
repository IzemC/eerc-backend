using ENOC.API.Middleware;
using ENOC.Infrastructure;
using ENOC.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    // .WriteTo.{ENOC_SIEM_Sink}() // TO BE CONFIGURED WITH ENOC GIT
    .CreateLogger();
builder.Host.UseSerilog();

// Infrastructure layer (Database, Identity, JWT, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR for real-time communication
builder.Services.AddSignalR();

// Notification service (registered here to avoid circular dependency with SignalR hub)
builder.Services.AddScoped<ENOC.Application.Interfaces.INotificationService, ENOC.API.Services.NotificationService>();

// Redis cache
var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of numbers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ENOC EERC API", Version = "v1" });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ENOC EERC API v1");
    });
}
else
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseSerilogRequestLogging();

app.UseCors("AllowAll");

// TODO: FOR TESTING ONLY - Remove fake auth and re-enable real authentication
// This injects a fake user so endpoints expecting user info still work
app.UseFakeAuth();

// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR hubs
app.MapHub<ENOC.API.Hubs.NotificationHub>("/hubs/notification");

// Seed database
await app.SeedDatabaseAsync();

app.Run();
