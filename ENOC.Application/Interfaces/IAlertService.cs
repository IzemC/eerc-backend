using ENOC.Application.DTOs.Alert;

namespace ENOC.Application.Interfaces;

public interface IAlertService
{
    Task<bool> SendAlertToTeamsAsync(SendAlertRequest request, CancellationToken cancellationToken = default);
}
