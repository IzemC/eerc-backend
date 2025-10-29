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

    // Vehicle status methods
    Task<ShiftReportVehicleStatusResponse?> AddVehicleStatusAsync(Guid shiftReportId, Guid vehicleId, string? description, CancellationToken cancellationToken = default);
    Task<List<ShiftReportVehicleStatusResponse>> AddBulkVehicleStatusAsync(Guid shiftReportId, AddBulkVehicleStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShiftReportVehicleStatusResponse>> GetVehicleStatusesByShiftReportIdAsync(Guid shiftReportId, CancellationToken cancellationToken = default);
    Task<bool> DeleteVehicleStatusAsync(Guid vehicleStatusId, CancellationToken cancellationToken = default);

    // Activities management
    Task<ShiftReportResponse?> UpdateActivitiesAsync(Guid shiftReportId, string activities, CancellationToken cancellationToken = default);
}

public interface ICrewVehicleListingService
{
    Task<CrewVehicleListingResponse?> CreateCrewVehicleListingAsync(CreateCrewVehicleListingRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<CrewVehicleListingResponse?> GetCrewVehicleListingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CrewVehicleListingResponse>> GetAllCrewVehicleListingsAsync(Guid? teamId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteCrewVehicleListingAsync(Guid id, CancellationToken cancellationToken = default);

    // Team status methods
    Task<TeamStatusResponse?> AddTeamStatusAsync(Guid crewVehicleListingId, AddTeamStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeamStatusResponse>> GetTeamStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTeamStatusAsync(Guid teamStatusId, CancellationToken cancellationToken = default);

    // Vehicle status methods
    Task<VehicleStatusResponse?> AddVehicleStatusAsync(Guid crewVehicleListingId, AddVehicleStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<VehicleStatusResponse>> GetVehicleStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default);
    Task<bool> DeleteVehicleStatusAsync(Guid vehicleStatusId, CancellationToken cancellationToken = default);

    // SCBA status methods
    Task<SCBAStatusResponse?> AddSCBAStatusAsync(Guid crewVehicleListingId, AddSCBAStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<SCBAStatusResponse>> GetSCBAStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSCBAStatusAsync(Guid scbaStatusId, CancellationToken cancellationToken = default);

    // Radio status methods
    Task<RadioStatusResponse?> AddRadioStatusAsync(Guid crewVehicleListingId, AddRadioStatusRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<RadioStatusResponse>> GetRadioStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRadioStatusAsync(Guid radioStatusId, CancellationToken cancellationToken = default);
}

public interface IInspectionService
{
    Task<InspectionResponse?> CreateInspectionAsync(CreateInspectionRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<InspectionResponse?> GetInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InspectionResponse>> GetAllInspectionsAsync(Guid? vehicleId = null, bool? isDefected = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteInspectionAsync(Guid id, CancellationToken cancellationToken = default);

    // Defect methods
    Task<InspectionDefectResponse?> AddDefectAsync(Guid inspectionId, string questionId, string questionText, string description, byte[]? image, string? imageFileName, string? imageContentType, long? imageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<InspectionDefectResponse>> GetDefectsByInspectionIdAsync(Guid inspectionId, CancellationToken cancellationToken = default);
    Task<(byte[] content, string fileName, string contentType)?> GetDefectImageAsync(Guid defectId, CancellationToken cancellationToken = default);
    Task<bool> DeleteDefectAsync(Guid defectId, CancellationToken cancellationToken = default);
}
