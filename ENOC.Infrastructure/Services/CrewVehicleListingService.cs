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

    // Team status methods
    public async Task<TeamStatusResponse?> AddTeamStatusAsync(Guid crewVehicleListingId, AddTeamStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate crew vehicle listing exists
            var form = await _unitOfWork.Repository<CrewVehicleListingForm>().GetByIdAsync(crewVehicleListingId, cancellationToken);
            if (form == null)
            {
                _logger.LogWarning("Crew vehicle listing {ListingId} not found", crewVehicleListingId);
                return null;
            }

            // Validate user exists
            var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", request.UserId);
                return null;
            }

            // Validate employee status exists
            var employeeStatus = await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(request.EmployeeStatusId, cancellationToken);
            if (employeeStatus == null)
            {
                _logger.LogWarning("Employee status {StatusId} not found", request.EmployeeStatusId);
                return null;
            }

            // Validate vehicle if provided
            Vehicle? vehicle = null;
            if (request.VehicleId.HasValue)
            {
                vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(request.VehicleId.Value, cancellationToken);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle {VehicleId} not found", request.VehicleId);
                    return null;
                }
            }

            var entry = new TeamStatusEntry
            {
                Id = Guid.NewGuid(),
                CrewVehicleListingFormId = crewVehicleListingId,
                UserId = request.UserId,
                EmployeeStatusId = request.EmployeeStatusId,
                VehicleId = request.VehicleId,
                Remarks = request.Remarks
            };

            await _unitOfWork.Repository<TeamStatusEntry>().AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Team status entry {EntryId} added to crew vehicle listing {ListingId}", entry.Id, crewVehicleListingId);

            return new TeamStatusResponse
            {
                Id = entry.Id,
                UserId = entry.UserId,
                UserName = user.UserName ?? "Unknown",
                EmployeeStatusId = entry.EmployeeStatusId,
                EmployeeStatusName = employeeStatus.Name,
                VehicleId = entry.VehicleId,
                VehicleName = vehicle?.Name,
                Remarks = entry.Remarks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team status entry");
            throw;
        }
    }

    public async Task<IEnumerable<TeamStatusResponse>> GetTeamStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default)
    {
        var entries = (await _unitOfWork.Repository<TeamStatusEntry>().GetAllAsync(cancellationToken))
            .Where(t => t.CrewVehicleListingFormId == crewVehicleListingId)
            .ToList();

        var responses = new List<TeamStatusResponse>();
        foreach (var entry in entries)
        {
            var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(entry.UserId, cancellationToken);
            var employeeStatus = await _unitOfWork.Repository<EmployeeStatus>().GetByIdAsync(entry.EmployeeStatusId, cancellationToken);
            var vehicle = entry.VehicleId.HasValue
                ? await _unitOfWork.Repository<Vehicle>().GetByIdAsync(entry.VehicleId.Value, cancellationToken)
                : null;

            responses.Add(new TeamStatusResponse
            {
                Id = entry.Id,
                UserId = entry.UserId,
                UserName = user?.UserName ?? "Unknown",
                EmployeeStatusId = entry.EmployeeStatusId,
                EmployeeStatusName = employeeStatus?.Name ?? "Unknown",
                VehicleId = entry.VehicleId,
                VehicleName = vehicle?.Name,
                Remarks = entry.Remarks
            });
        }

        return responses;
    }

    public async Task<bool> DeleteTeamStatusAsync(Guid teamStatusId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.Repository<TeamStatusEntry>().GetByIdAsync(teamStatusId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        _unitOfWork.Repository<TeamStatusEntry>().Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Team status entry {EntryId} deleted", teamStatusId);

        return true;
    }

    // Vehicle status methods
    public async Task<VehicleStatusResponse?> AddVehicleStatusAsync(Guid crewVehicleListingId, AddVehicleStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate crew vehicle listing exists
            var form = await _unitOfWork.Repository<CrewVehicleListingForm>().GetByIdAsync(crewVehicleListingId, cancellationToken);
            if (form == null)
            {
                _logger.LogWarning("Crew vehicle listing {ListingId} not found", crewVehicleListingId);
                return null;
            }

            // Validate vehicle exists
            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(request.VehicleId, cancellationToken);
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle {VehicleId} not found", request.VehicleId);
                return null;
            }

            var entry = new FireVehicleStatusEntry
            {
                Id = Guid.NewGuid(),
                CrewVehicleListingFormId = crewVehicleListingId,
                VehicleId = request.VehicleId,
                In = request.In,
                Out = request.Out,
                Rs = request.Rs,
                Remarks = request.Remarks
            };

            await _unitOfWork.Repository<FireVehicleStatusEntry>().AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vehicle status entry {EntryId} added to crew vehicle listing {ListingId}", entry.Id, crewVehicleListingId);

            return new VehicleStatusResponse
            {
                Id = entry.Id,
                VehicleId = entry.VehicleId,
                VehicleName = vehicle.Name,
                In = entry.In,
                Out = entry.Out,
                Rs = entry.Rs,
                Remarks = entry.Remarks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding vehicle status entry");
            throw;
        }
    }

    public async Task<IEnumerable<VehicleStatusResponse>> GetVehicleStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default)
    {
        var entries = (await _unitOfWork.Repository<FireVehicleStatusEntry>().GetAllAsync(cancellationToken))
            .Where(v => v.CrewVehicleListingFormId == crewVehicleListingId)
            .ToList();

        var responses = new List<VehicleStatusResponse>();
        foreach (var entry in entries)
        {
            var vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(entry.VehicleId, cancellationToken);

            responses.Add(new VehicleStatusResponse
            {
                Id = entry.Id,
                VehicleId = entry.VehicleId,
                VehicleName = vehicle?.Name ?? "Unknown",
                In = entry.In,
                Out = entry.Out,
                Rs = entry.Rs,
                Remarks = entry.Remarks
            });
        }

        return responses;
    }

    public async Task<bool> DeleteVehicleStatusAsync(Guid vehicleStatusId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.Repository<FireVehicleStatusEntry>().GetByIdAsync(vehicleStatusId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        _unitOfWork.Repository<FireVehicleStatusEntry>().Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vehicle status entry {EntryId} deleted", vehicleStatusId);

        return true;
    }

    // SCBA status methods
    public async Task<SCBAStatusResponse?> AddSCBAStatusAsync(Guid crewVehicleListingId, AddSCBAStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate crew vehicle listing exists
            var form = await _unitOfWork.Repository<CrewVehicleListingForm>().GetByIdAsync(crewVehicleListingId, cancellationToken);
            if (form == null)
            {
                _logger.LogWarning("Crew vehicle listing {ListingId} not found", crewVehicleListingId);
                return null;
            }

            // Validate SCBA exists
            var scba = await _unitOfWork.Repository<SCBA>().GetByIdAsync(request.SCBAId, cancellationToken);
            if (scba == null)
            {
                _logger.LogWarning("SCBA {SCBAId} not found", request.SCBAId);
                return null;
            }

            // Validate vehicle if provided
            Vehicle? vehicle = null;
            if (request.VehicleId.HasValue)
            {
                vehicle = await _unitOfWork.Repository<Vehicle>().GetByIdAsync(request.VehicleId.Value, cancellationToken);
                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle {VehicleId} not found", request.VehicleId);
                    return null;
                }
            }

            var entry = new SCBAStatusEntry
            {
                Id = Guid.NewGuid(),
                CrewVehicleListingFormId = crewVehicleListingId,
                SCBAId = request.SCBAId,
                VehicleId = request.VehicleId,
                CylinderPressure = request.CylinderPressure,
                In = request.In,
                Out = request.Out,
                Rs = request.Rs,
                Remarks = request.Remarks
            };

            await _unitOfWork.Repository<SCBAStatusEntry>().AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("SCBA status entry {EntryId} added to crew vehicle listing {ListingId}", entry.Id, crewVehicleListingId);

            return new SCBAStatusResponse
            {
                Id = entry.Id,
                SCBAId = entry.SCBAId,
                SCBAName = scba.Name,
                VehicleId = entry.VehicleId,
                VehicleName = vehicle?.Name,
                CylinderPressure = entry.CylinderPressure,
                In = entry.In,
                Out = entry.Out,
                Rs = entry.Rs,
                Remarks = entry.Remarks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding SCBA status entry");
            throw;
        }
    }

    public async Task<IEnumerable<SCBAStatusResponse>> GetSCBAStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default)
    {
        var entries = (await _unitOfWork.Repository<SCBAStatusEntry>().GetAllAsync(cancellationToken))
            .Where(s => s.CrewVehicleListingFormId == crewVehicleListingId)
            .ToList();

        var responses = new List<SCBAStatusResponse>();
        foreach (var entry in entries)
        {
            var scba = await _unitOfWork.Repository<SCBA>().GetByIdAsync(entry.SCBAId, cancellationToken);
            var vehicle = entry.VehicleId.HasValue
                ? await _unitOfWork.Repository<Vehicle>().GetByIdAsync(entry.VehicleId.Value, cancellationToken)
                : null;

            responses.Add(new SCBAStatusResponse
            {
                Id = entry.Id,
                SCBAId = entry.SCBAId,
                SCBAName = scba?.Name ?? "Unknown",
                VehicleId = entry.VehicleId,
                VehicleName = vehicle?.Name,
                CylinderPressure = entry.CylinderPressure,
                In = entry.In,
                Out = entry.Out,
                Rs = entry.Rs,
                Remarks = entry.Remarks
            });
        }

        return responses;
    }

    public async Task<bool> DeleteSCBAStatusAsync(Guid scbaStatusId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.Repository<SCBAStatusEntry>().GetByIdAsync(scbaStatusId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        _unitOfWork.Repository<SCBAStatusEntry>().Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SCBA status entry {EntryId} deleted", scbaStatusId);

        return true;
    }

    // Radio status methods
    public async Task<RadioStatusResponse?> AddRadioStatusAsync(Guid crewVehicleListingId, AddRadioStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate crew vehicle listing exists
            var form = await _unitOfWork.Repository<CrewVehicleListingForm>().GetByIdAsync(crewVehicleListingId, cancellationToken);
            if (form == null)
            {
                _logger.LogWarning("Crew vehicle listing {ListingId} not found", crewVehicleListingId);
                return null;
            }

            // Validate radio exists
            var radio = await _unitOfWork.Repository<Radio>().GetByIdAsync(request.RadioId, cancellationToken);
            if (radio == null)
            {
                _logger.LogWarning("Radio {RadioId} not found", request.RadioId);
                return null;
            }

            var entry = new RadioStatusEntry
            {
                Id = Guid.NewGuid(),
                CrewVehicleListingFormId = crewVehicleListingId,
                RadioId = request.RadioId,
                In = request.In,
                Out = request.Out,
                Rs = request.Rs,
                Remarks = request.Remarks
            };

            await _unitOfWork.Repository<RadioStatusEntry>().AddAsync(entry, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Radio status entry {EntryId} added to crew vehicle listing {ListingId}", entry.Id, crewVehicleListingId);

            return new RadioStatusResponse
            {
                Id = entry.Id,
                RadioId = entry.RadioId,
                RadioName = radio.Name,
                In = entry.In,
                Out = entry.Out,
                Rs = entry.Rs,
                Remarks = entry.Remarks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding radio status entry");
            throw;
        }
    }

    public async Task<IEnumerable<RadioStatusResponse>> GetRadioStatusesByListingIdAsync(Guid crewVehicleListingId, CancellationToken cancellationToken = default)
    {
        var entries = (await _unitOfWork.Repository<RadioStatusEntry>().GetAllAsync(cancellationToken))
            .Where(r => r.CrewVehicleListingFormId == crewVehicleListingId)
            .ToList();

        var responses = new List<RadioStatusResponse>();
        foreach (var entry in entries)
        {
            var radio = await _unitOfWork.Repository<Radio>().GetByIdAsync(entry.RadioId, cancellationToken);

            responses.Add(new RadioStatusResponse
            {
                Id = entry.Id,
                RadioId = entry.RadioId,
                RadioName = radio?.Name ?? "Unknown",
                In = entry.In,
                Out = entry.Out,
                Rs = entry.Rs,
                Remarks = entry.Remarks
            });
        }

        return responses;
    }

    public async Task<bool> DeleteRadioStatusAsync(Guid radioStatusId, CancellationToken cancellationToken = default)
    {
        var entry = await _unitOfWork.Repository<RadioStatusEntry>().GetByIdAsync(radioStatusId, cancellationToken);
        if (entry == null)
        {
            return false;
        }

        _unitOfWork.Repository<RadioStatusEntry>().Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Radio status entry {EntryId} deleted", radioStatusId);

        return true;
    }
}
