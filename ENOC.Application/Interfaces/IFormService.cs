using ENOC.Application.DTOs.ShiftReport;
using ENOC.Application.DTOs.CrewVehicleListing;
using ENOC.Application.DTOs.Inspection;

namespace ENOC.Application.Interfaces;

public interface IShiftReportService
{
    Task<ShiftReportResponse?> CreateShiftReportAsync(CreateShiftReportRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ShiftReportResponse?> GetShiftReportByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftReportResponse>> GetAllShiftReportsAsync(Guid? teamId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteShiftReportAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICrewVehicleListingService
{
    Task<CrewVehicleListingResponse?> CreateCrewVehicleListingAsync(CreateCrewVehicleListingRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<CrewVehicleListingResponse?> GetCrewVehicleListingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CrewVehicleListingResponse>> GetAllCrewVehicleListingsAsync(Guid? teamId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteCrewVehicleListingAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IInspectionService
{
    Task<InspectionResponse?> CreateInspectionAsync(CreateInspectionRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<InspectionResponse?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InspectionResponse>> GetAllInspectionsAsync(Guid? vehicleId = null, bool? isDefected = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteInspectionAsync(Guid id, CancellationToken cancellationToken = default);
}
