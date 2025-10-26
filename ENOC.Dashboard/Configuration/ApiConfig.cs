namespace ENOC.Dashboard.Configuration;

public class ApiConfig
{
    public string BaseUrl { get; set; } = "https://localhost:7087";
    public int Timeout { get; set; } = 30;
}
