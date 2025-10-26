namespace ENOC.Dashboard.Models;

public class IncidentFormState
{
    // Step 1 data
    public Guid? IncidentTypeId { get; set; }
    public string? IncidentTypeName { get; set; }
    public Guid? BusinessUnitId { get; set; }
    public string? BusinessUnitName { get; set; }

    // Step 2 data
    public Guid? TankId { get; set; }
    public string? TankName { get; set; }
    public string? Action { get; set; } // "Notify team" or "Stand down"

    // Step 3 data
    public List<Guid> TeamIds { get; set; } = new();
    public List<string> TeamNames { get; set; } = new();
    public Guid? MessageId { get; set; }
    public string? MessageText { get; set; }
    public string? CustomMessage { get; set; }

    // Additional details
    public string? ReporterName { get; set; }
    public string? ReporterContactDetails { get; set; }

    // Final action
    public string? FinalAction { get; set; } // "Close incident" or "Stand down"

    public void Reset()
    {
        IncidentTypeId = null;
        IncidentTypeName = null;
        BusinessUnitId = null;
        BusinessUnitName = null;
        TankId = null;
        TankName = null;
        Action = null;
        TeamIds.Clear();
        TeamNames.Clear();
        MessageId = null;
        MessageText = null;
        CustomMessage = null;
        ReporterName = null;
        ReporterContactDetails = null;
        FinalAction = null;
    }
}
