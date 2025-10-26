namespace ENOC.Application.Configuration;

public class FeatureFlags
{
    public bool UseActiveDirectory { get; set; }
    public bool UseSmsService { get; set; }
    public bool UseEmailService { get; set; }
    public bool UseFcmService { get; set; }
    public bool UseWeatherService { get; set; }
}
