namespace ENOC.Application.Configuration;

public class JwtConfig
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int TokenValidaty { get; set; } // in days
    public int RefreshTokenValidaty { get; set; } // in days
}
