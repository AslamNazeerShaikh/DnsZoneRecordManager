namespace DnsZoneRecordManager.Models;

/// <summary>Error page view model.</summary>
public class ErrorViewModel
{
    /// <summary>Request identifier for support.</summary>
    public string? RequestId { get; set; }

    /// <summary>True when a request id is available to show.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
