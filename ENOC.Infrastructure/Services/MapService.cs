using ENOC.Application.DTOs.Map;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using ENOC.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class MapService : IMapService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MapService> _logger;

    public MapService(IUnitOfWork unitOfWork, ILogger<MapService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MapDataResponse> GetMapDataAsync(Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all tanks with optional business unit filter
            var tanksQuery = (await _unitOfWork.Repository<Tank>().GetAllAsync(cancellationToken))
                .Where(t => !t.IsDeleted);

            if (businessUnitId.HasValue)
            {
                tanksQuery = tanksQuery.Where(t => t.BusinessUnitId == businessUnitId.Value);
            }

            var tanks = tanksQuery.ToList();

            // Get all open incidents (Status is enum, check for OPEN and TEST)
            var openIncidents = (await _unitOfWork.Repository<Incident>().GetAllAsync(cancellationToken))
                .Where(i => i.Status != IncidentStatus.CLOSE && i.TankId.HasValue)
                .GroupBy(i => i.TankId.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            // Get business units for names
            var businessUnits = (await _unitOfWork.Repository<BusinessUnit>().GetAllAsync(cancellationToken))
                .ToDictionary(bu => bu.Id, bu => bu.Name);

            var tankMarkers = tanks.Select(tank =>
            {
                var hasOpenIncident = openIncidents.ContainsKey(tank.Id);
                var openIncidentCount = hasOpenIncident ? openIncidents[tank.Id] : 0;

                // Parse Location string for lat/long if available (format: "lat,long")
                decimal latitude = 0;
                decimal longitude = 0;
                if (!string.IsNullOrEmpty(tank.Location) && tank.Location.Contains(','))
                {
                    var coords = tank.Location.Split(',');
                    if (coords.Length == 2)
                    {
                        decimal.TryParse(coords[0].Trim(), out latitude);
                        decimal.TryParse(coords[1].Trim(), out longitude);
                    }
                }

                return new TankMapMarker
                {
                    TankId = tank.Id,
                    TankName = tank.Name,
                    TankNumber = tank.TankNumber.ToString(),
                    BusinessUnit = businessUnits.ContainsKey(tank.BusinessUnitId)
                        ? businessUnits[tank.BusinessUnitId]
                        : "Unknown",
                    Latitude = latitude,
                    Longitude = longitude,
                    HasOpenIncident = hasOpenIncident,
                    OpenIncidentCount = openIncidentCount,
                    PolygonCoordinates = null // Future: Store polygon data in DB
                };
            }).ToList();

            var response = new MapDataResponse
            {
                Tanks = tankMarkers,
                TotalTanks = tankMarkers.Count,
                TanksWithOpenIncidents = tankMarkers.Count(t => t.HasOpenIncident)
            };

            _logger.LogInformation("Retrieved map data: {TotalTanks} tanks, {TanksWithIncidents} with open incidents",
                response.TotalTanks, response.TanksWithOpenIncidents);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving map data");
            throw;
        }
    }
}
