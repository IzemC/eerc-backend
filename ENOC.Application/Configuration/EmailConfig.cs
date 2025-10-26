namespace ENOC.Application.Configuration;

public class EmailConfig
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
}
