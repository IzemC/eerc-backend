namespace ENOC.Application.DTOs.Alert;

public class SendAlertRequest
{
    public string Message { get; set; } = string.Empty;
    public List<Guid> TeamIds { get; set; } = new();
    public bool SendEmail { get; set; } = true;
    public bool SendSms { get; set; } = true;
    public bool SendPushNotification { get; set; } = true;
}
