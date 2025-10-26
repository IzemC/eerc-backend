using System.Security.Claims;

namespace ENOC.API.Middleware;

/// <summary>
/// Middleware for testing that injects a fake authenticated user
/// TODO: Remove this in production - only for testing data integration
/// </summary>
public class FakeAuthMiddleware
{
    private readonly RequestDelegate _next;

    public FakeAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Create a fake user with claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"), // Fake user ID
            new Claim(ClaimTypes.Name, "dev@enoc.local"),
            new Claim(ClaimTypes.Email, "dev@enoc.local"),
            new Claim(ClaimTypes.GivenName, "Developer"),
            new Claim(ClaimTypes.Surname, "Account"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("EmployeeId", "DEV001"),
            new Claim("TeamId", "00000000-0000-0000-0000-000000000001")
        };

        var identity = new ClaimsIdentity(claims, "FakeAuth");
        var principal = new ClaimsPrincipal(identity);

        context.User = principal;

        await _next(context);
    }
}

public static class FakeAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseFakeAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FakeAuthMiddleware>();
    }
}
