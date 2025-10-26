namespace ENOC.Dashboard.Models;

public class SendAlertRequest
{
    public string Message { get; set; } = string.Empty;
    public List<Guid> TeamIds { get; set; } = new();
    public bool SendPushNotification { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
}
