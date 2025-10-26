using ENOC.Application.DTOs.Map;

namespace ENOC.Application.Interfaces;

public interface IMapService
{
    Task<MapDataResponse> GetMapDataAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default);
}
