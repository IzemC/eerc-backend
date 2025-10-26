using ENOC.Application.DTOs.Incident;
using ENOC.Domain.Enums;

namespace ENOC.Application.Interfaces;

public interface IIncidentService
{
    Task<IncidentResponse?> CreateIncidentAsync(CreateIncidentRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<IncidentResponse?> GetIncidentByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<IncidentResponse>> GetAllIncidentsAsync(IncidentStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<IncidentResponse?> UpdateIncidentAsync(Guid id, UpdateIncidentRequest request, CancellationToken cancellationToken = default);
    Task<bool> CloseIncidentAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AcknowledgeIncidentAsync(Guid incidentId, Guid userId, AcknowledgeIncidentRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<IncidentAcknowledgementResponse>> GetIncidentAcknowledgementsAsync(Guid incidentId, CancellationToken cancellationToken = default);
}
