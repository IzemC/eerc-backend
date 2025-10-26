using ENOC.Application.Interfaces;
using ENOC.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ENOC.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LookupController> _logger;

    public LookupController(IUnitOfWork unitOfWork, ILogger<LookupController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all business units
    /// </summary>
    [HttpGet("business-units")]
    public async Task<ActionResult> GetBusinessUnits(CancellationToken cancellationToken)
    {
        try
        {
            var units = await _unitOfWork.Repository<BusinessUnit>().GetAllAsync(cancellationToken);
            var result = units.Where(u => !u.IsDeleted).Select(u => new
            {
                u.Id,
                u.Name
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business units");
            return StatusCode(500, new { message = "An error occurred while retrieving business units" });
        }
    }

    /// <summary>
    /// Get all incident types
    /// </summary>
    [HttpGet("incident-types")]
    public async Task<ActionResult> GetIncidentTypes(CancellationToken cancellationToken)
    {
        try
        {
            var types = await _unitOfWork.Repository<IncidentType>().GetAllAsync(cancellationToken);
            var result = types.Where(t => !t.IsDeleted).Select(t => new
            {
                t.Id,
                t.Name,
                t.Image,
                t.AlarmPA,
                t.AlarmVoice
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident types");
            return StatusCode(500, new { message = "An error occurred while retrieving incident types" });
        }
    }

    /// <summary>
    /// Get all messages
    /// </summary>
    [HttpGet("messages")]
    public async Task<ActionResult> GetMessages(CancellationToken cancellationToken)
    {
        try
        {
            var messages = await _unitOfWork.Repository<Message>().GetAllAsync(cancellationToken);
            var result = messages.Where(m => !m.IsDeleted).Select(m => new
            {
                m.Id,
                m.Description
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving messages");
            return StatusCode(500, new { message = "An error occurred while retrieving messages" });
        }
    }

    /// <summary>
    /// Get all tanks (optionally filtered by business unit)
    /// </summary>
    [HttpGet("tanks")]
    public async Task<ActionResult> GetTanks([FromQuery] Guid? businessUnitId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var tanks = await _unitOfWork.Repository<Tank>().GetAllAsync(cancellationToken);
            var filtered = tanks.Where(t => !t.IsDeleted);

            if (businessUnitId.HasValue)
            {
                filtered = filtered.Where(t => t.BusinessUnitId == businessUnitId.Value);
            }

            var result = filtered.Select(t => new
            {
                t.Id,
                t.Name,
                t.TankNumber,
                t.BusinessUnitId,
                t.Location,
                t.ERG
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tanks");
            return StatusCode(500, new { message = "An error occurred while retrieving tanks" });
        }
    }

    /// <summary>
    /// Get all teams
    /// </summary>
    [HttpGet("teams")]
    public async Task<ActionResult> GetTeams(CancellationToken cancellationToken)
    {
        try
        {
            var teams = await _unitOfWork.Repository<Team>().GetAllAsync(cancellationToken);
            var result = teams.Where(t => !t.IsDeleted).Select(t => new
            {
                t.Id,
                t.Name,
                t.Color,
                t.ColorName
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams");
            return StatusCode(500, new { message = "An error occurred while retrieving teams" });
        }
    }

    /// <summary>
    /// Get all vehicles
    /// </summary>
    [HttpGet("vehicles")]
    public async Task<ActionResult> GetVehicles(CancellationToken cancellationToken)
    {
        try
        {
            var vehicles = await _unitOfWork.Repository<Vehicle>().GetAllAsync(cancellationToken);
            var result = vehicles.Where(v => !v.IsDeleted).Select(v => new
            {
                v.Id,
                v.Name,
                v.PlateNumber,
                v.Image
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            return StatusCode(500, new { message = "An error occurred while retrieving vehicles" });
        }
    }

    /// <summary>
    /// Get all SCBAs
    /// </summary>
    [HttpGet("scbas")]
    public async Task<ActionResult> GetSCBAs(CancellationToken cancellationToken)
    {
        try
        {
            var scbas = await _unitOfWork.Repository<SCBA>().GetAllAsync(cancellationToken);
            var result = scbas.Where(s => !s.IsDeleted).Select(s => new
            {
                s.Id,
                s.Name
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SCBAs");
            return StatusCode(500, new { message = "An error occurred while retrieving SCBAs" });
        }
    }

    /// <summary>
    /// Get all radios
    /// </summary>
    [HttpGet("radios")]
    public async Task<ActionResult> GetRadios(CancellationToken cancellationToken)
    {
        try
        {
            var radios = await _unitOfWork.Repository<Radio>().GetAllAsync(cancellationToken);
            var result = radios.Where(r => !r.IsDeleted).Select(r => new
            {
                r.Id,
                r.Name,
                r.Location
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving radios");
            return StatusCode(500, new { message = "An error occurred while retrieving radios" });
        }
    }

    /// <summary>
    /// Get all EERC positions
    /// </summary>
    [HttpGet("eerc-positions")]
    public async Task<ActionResult> GetEercPositions(CancellationToken cancellationToken)
    {
        try
        {
            var positions = await _unitOfWork.Repository<EercPosition>().GetAllAsync(cancellationToken);
            var result = positions.Where(p => !p.IsDeleted).Select(p => new
            {
                p.Id,
                p.Name,
                p.Symbol
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving EERC positions");
            return StatusCode(500, new { message = "An error occurred while retrieving EERC positions" });
        }
    }

    /// <summary>
    /// Get all employee statuses
    /// </summary>
    [HttpGet("employee-statuses")]
    public async Task<ActionResult> GetEmployeeStatuses(CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _unitOfWork.Repository<EmployeeStatus>().GetAllAsync(cancellationToken);
            var result = statuses.Where(s => !s.IsDeleted).Select(s => new
            {
                s.Id,
                s.Name,
                s.Symbol
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving employee statuses" });
        }
    }

    /// <summary>
    /// Get all EERC statuses
    /// </summary>
    [HttpGet("eerc-statuses")]
    public async Task<ActionResult> GetEercStatuses(CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _unitOfWork.Repository<EercStatus>().GetAllAsync(cancellationToken);
            var result = statuses.Where(s => !s.IsDeleted).Select(s => new
            {
                s.Id,
                s.Name,
                s.Symbol
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving EERC statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving EERC statuses" });
        }
    }
}
