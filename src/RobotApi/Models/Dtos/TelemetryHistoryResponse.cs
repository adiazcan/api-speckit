namespace RobotApi.Models.Dtos;

/// <summary>
/// Response model for historical telemetry queries with pagination metadata.
/// </summary>
public class TelemetryHistoryResponse
{
    /// <summary>
    /// Array of telemetry snapshots for the requested page.
    /// </summary>
    public TelemetryResponse[] Data { get; set; } = Array.Empty<TelemetryResponse>();

    /// <summary>
    /// Total number of telemetry records matching the query (across all pages).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of records per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there is a next page available.
    /// </summary>
    public bool HasNextPage => Page * PageSize < TotalCount;

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
