using ENOC.Application.DTOs.CrewVehicleListing;
using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ENOC.Infrastructure.Services;

public class CrewVehicleListingService : ICrewVehicleListingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CrewVehicleListingService> _logger;

    public CrewVehicleListingService(IUnitOfWork unitOfWork, ILogger<CrewVehicleListingService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CrewVehicleListingResponse?> CreateCrewVehicleListingAsync(CreateCrewVehicleListingRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate crew vehicle listing form counter and ID
            var formRepo = _unitOfWork.Repository<CrewVehicleListingForm>();
            var lastForm = (await formRepo.GetAllAsync(cancellationToken))
                .OrderByDescending(f => f.CrewVehicleListingFormCounter)
                .FirstOrDefault();

            var counter = (lastForm?.CrewVehicleListingFormCounter ?? 0) + 1;
            var formId = $"CVL-{DateTime.UtcNow:yyyyMMdd}-{counter:D4}";

            var form = new CrewVehicleListingForm
            {
                Id = Guid.NewGuid(),
                CrewVehicleListingFormId = formId,
                CrewVehicleListingFormCounter = counter,
                UserId = userId,
                TeamId = request.TeamId,
                From = request.From,
                To = request.To,
                Name = request.Name,
                Details = request.Details,
                CreatedAt = DateTime.UtcNow
            };

            await formRepo.AddAsync(form, cancellationToken);

            // Add team status entries
            var teamStatusRepo = _unitOfWork.Repository<TeamStatusEntry>();
            foreach (var teamStatus in request.TeamStatuses)
            {
                var entry = new TeamStatusEntry
                {
                    Id = Guid.NewGuid(),
                    CrewVehicleListingFormId = form.Id,
                    UserId = teamStatus.UserId,
                    EmployeeStatusId = teamStatus.EmployeeStatusId,
                    VehicleId = teamStatus.VehicleId,
                    Remarks = teamStatus.Remarks
                };
                await teamStatusRepo.AddAsync(entry, cancellationToken);
            }

            // Add vehicle status entries
            var vehicleStatusRepo = _unitOfWork.Repository<FireVehicleStatusEntry>();
            foreach (var vehicleStatus in request.VehicleStatuses)
            {
                var entry = new FireVehicleStatusEntry
                {
                    Id = Guid.NewGuid(),
                    CrewVehicleListingFormId = form.Id,
                    VehicleId = vehicleStatus.VehicleId,
                    In = vehicleStatus.In,
                    Out = vehicleStatus.Out,
                    Rs = vehicleStatus.Rs,
                    Remarks = vehicleStatus.Remarks
                };
                await vehicleStatusRepo.AddAsync(entry, cancellationToken);
            }

            // Add SCBA status entries
            var scbaStatusRepo = _unitOfWork.Repository<SCBAStatusEntry>();
            foreach (var scbaStatus in request.SCBAStatuses)
            {
                var entry = new SCBAStatusEntry
                {
                    Id = Guid.NewGuid(),
                    CrewVehicleListingFormId = form.Id,
                    SCBAId = scbaStatus.SCBAId,
                    VehicleId = scbaStatus.VehicleId,
                    CylinderPressure = scbaStatus.CylinderPressure,
                    In = scbaStatus.In,
                    Out = scbaStatus.Out,
                    Rs = scbaStatus.Rs,
                    Remarks = scbaStatus.Remarks
                };
                await scbaStatusRepo.AddAsync(entry, cancellationToken);
            }

            // Add radio status entries
            var radioStatusRepo = _unitOfWork.Repository<RadioStatusEntry>();
            foreach (var radioStatus in request.RadioStatuses)
            {
                var entry = new RadioStatusEntry
                {
                    Id = Guid.NewGuid(),
                    CrewVehicleListingFormId = form.Id,
                    RadioId = radioStatus.RadioId,
                    In = radioStatus.In,
                    Out = radioStatus.Out,
                    Rs = radioStatus.Rs,
                    Remarks = radioStatus.Remarks
                };
                await radioStatusRepo.AddAsync(entry, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Crew vehicle listing {FormId} created by user {UserId}", formId, userId);

            return await GetCrewVehicleListingByIdAsync(form.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating crew vehicle listing");
            throw;
        }
    }

    public async Task<CrewVehicleListingResponse?> GetCrewVehicleListingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var form = await _unitOfWork.Repository<CrewVehicleListingForm>().GetByIdAsync(id, cancellationToken);
        if (form == null)
        {
            return null;
        }

        var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(form.UserId, cancellationToken);
        var team = await _unitOfWork.Repository<Team>().GetByIdAsync(form.TeamId, cancellationToken);

        // Get all status entries for this form
        var teamStatuses = (await _unitOfWork.Repository<TeamStatusEntry>().GetAllAsync(cancellationToken))
            .Where(t => t.CrewVehicleListingFormId == form.Id)
            .ToList();

        var vehicleStatuses = (await _unitOfWork.Repository<FireVehicleStatusEntry>().GetAllAsync(cancellationToken))
            .Where(v => v.CrewVehicleListingFormId == form.Id)
            .ToList();

        var scbaStatuses = (await _unitOfWork.Repository<SCBAStatusEntry>().GetAllAsync(cancellationToken))
            .Where(s => s.CrewVehicleListingFormId == form.Id)
            .ToList();

        var radioStatuses = (await _unitOfWork.Repository<RadioStatusEntry>().GetAllAsync(cancellationToken))
            .Where(r => r.CrewVehicleListingFormId == form.Id)
            .ToList();

        var response = new CrewVehicleListingResponse
        {
            Id = form.Id,
            CrewVehicleListingFormId = form.CrewVehicleListingFormId,
            CrewVehicleListingFormCounter = form.CrewVehicleListingFormCounter,
            UserId = form.UserId,
            UserName = user?.UserName ?? "Unknown",
            UserEmployeeId = user?.EmployeeId ?? "Unknown",
            TeamId = form.TeamId,
            TeamName = team?.Name ?? "Unknown",
            From = form.From,
            To = form.To,
            Name = form.Name,
            Details = form.Details,
            CreatedAt = form.CreatedAt
        };

        // Map team statuses
        foreach (var teamStatus in teamStatuses)
        {
            var statusUser = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(teamStatus.UserId, cancellationToken);
            var employeeStatus = await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(teamStatus.EmployeeStatusId, cancellationToken);
            var vehicle = teamStatus.VehicleId.HasValue
                ? await _unitOfWork.Repository<Vehicle>().GetByIdAsync(teamStatus.VehicleId.Value, cancellationToken)
                : null;

            response.TeamStatuses.Add(new TeamStatusResponse
            {
                Id = teamStatus.Id,
                UserId = teamStatus.UserId,
                UserName = statusUser?.UserName ?? "Unknown",
                EmployeeStatusId = teamStatus.EmployeeStatusId,
                EmployeeStatusName = employeeStatus?.Name ?? "Unknown",
                VehicleId = teamStatus.VehicleId,
                VehicleName = vehicle?.Name,
                Remarks = teamStatus.Remarks
            });
        }

        // Map vehicle statuses
        foreach (var vehicleStatus in vehicleStatuses)
        {
            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(vehicleStatus.VehicleId, cancellationToken);

            response.VehicleStatuses.Add(new VehicleStatusResponse
            {
                Id = vehicleStatus.Id,
                VehicleId = vehicleStatus.VehicleId,
                VehicleName = vehicle?.Name ?? "Unknown",
                In = vehicleStatus.In,
                Out = vehicleStatus.Out,
                Rs = vehicleStatus.Rs,
                Remarks = vehicleStatus.Remarks
            });
        }

        // Map SCBA statuses
        foreach (var scbaStatus in scbaStatuses)
        {
            var scba = await _unitOfWork.Repository<SCBA>().GetByIdAsync(scbaStatus.SCBAId, cancellationToken);
            var vehicle = scbaStatus.VehicleId.HasValue
                ? await _unitOfWork.Repository<Vehicle>().GetByIdAsync(scbaStatus.VehicleId.Value, cancellationToken)
                : null;

            response.SCBAStatuses.Add(new SCBAStatusResponse
            {
                Id = scbaStatus.Id,
                SCBAId = scbaStatus.SCBAId,
                SCBAName = scba?.Name ?? "Unknown",
                VehicleId = scbaStatus.VehicleId,
                VehicleName = vehicle?.Name,
                CylinderPressure = scbaStatus.CylinderPressure,
                In = scbaStatus.In,
                Out = scbaStatus.Out,
                Rs = scbaStatus.Rs,
                Remarks = scbaStatus.Remarks
            });
        }

        // Map radio statuses
        foreach (var radioStatus in radioStatuses)
        {
            var radio = await _unitOfWork.Repository<Radio>().GetByIdAsync(radioStatus.RadioId, cancellationToken);

            response.RadioStatuses.Add(new RadioStatusResponse
            {
                Id = radioStatus.Id,
                RadioId = radioStatus.RadioId,
                RadioName = radio?.Name ?? "Unknown",
                In = radioStatus.In,
                Out = radioStatus.Out,
                Rs = radioStatus.Rs,
                Remarks = radioStatus.Remarks
            });
        }

        return response;
    }

    public async Task<IEnumerable<CrewVehicleListingResponse>> GetAllCrewVehicleListingsAsync(Guid? teamId = null, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var forms = await _unitOfWork.Repository<CrewVehicleListingForm>().GetAllAsync(cancellationToken);

        if (teamId.HasValue)
        {
            forms = forms.Where(f => f.TeamId == teamId.Value);
        }

        if (fromDate.HasValue)
        {
            forms = forms.Where(f => f.From >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            forms = forms.Where(f => f.To <= toDate.Value);
        }

        var responses = new List<CrewVehicleListingResponse>();
        foreach (var form in forms.OrderByDescending(f => f.CreatedAt))
        {
            var response = await GetCrewVehicleListingByIdAsync(form.Id, cancellationToken);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<bool> DeleteCrewVehicleListingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var form = await _unitOfWork.Repository<CrewVehicleListingForm>().GetByIdAsync(id, cancellationToken);
        if (form == null)
        {
            return false;
        }

        // Get all related status entries
        var teamStatuses = (await _unitOfWork.Repository<TeamStatusEntry>().GetAllAsync(cancellationToken))
            .Where(t => t.CrewVehicleListingFormId == form.Id);
        var vehicleStatuses = (await _unitOfWork.Repository<FireVehicleStatusEntry>().GetAllAsync(cancellationToken))
            .Where(v => v.CrewVehicleListingFormId == form.Id);
        var scbaStatuses = (await _unitOfWork.Repository<SCBAStatusEntry>().GetAllAsync(cancellationToken))
            .Where(s => s.CrewVehicleListingFormId == form.Id);
        var radioStatuses = (await _unitOfWork.Repository<RadioStatusEntry>().GetAllAsync(cancellationToken))
            .Where(r => r.CrewVehicleListingFormId == form.Id);

        // Remove all status entries
        foreach (var entry in teamStatuses)
        {
            _unitOfWork.Repository<TeamStatusEntry>().Remove(entry);
        }
        foreach (var entry in vehicleStatuses)
        {
            _unitOfWork.Repository<FireVehicleStatusEntry>().Remove(entry);
        }
        foreach (var entry in scbaStatuses)
        {
            _unitOfWork.Repository<SCBAStatusEntry>().Remove(entry);
        }
        foreach (var entry in radioStatuses)
        {
            _unitOfWork.Repository<RadioStatusEntry>().Remove(entry);
        }

        // Remove the form
        _unitOfWork.Repository<CrewVehicleListingForm>().Remove(form);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Crew vehicle listing {FormId} deleted", form.CrewVehicleListingFormId);

        return true;
    }
}
