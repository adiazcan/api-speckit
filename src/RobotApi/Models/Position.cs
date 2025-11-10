namespace RobotApi.Models;

/// <summary>
/// Represents robot position coordinates.
/// Supports both GPS coordinates and local coordinate systems.
/// </summary>
public sealed class Position
{
    /// <summary>
    /// Gets or sets the position type ("gps" or "local").
    /// </summary>
    public required string Type { get; set; }

    // GPS Coordinates
    /// <summary>
    /// Gets or sets the latitude (GPS only, -90 to 90).
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude (GPS only, -180 to 180).
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Gets or sets the altitude in meters (GPS only).
    /// </summary>
    public double? Altitude { get; set; }

    // Local Coordinates
    /// <summary>
    /// Gets or sets the X coordinate (local only).
    /// </summary>
    public double? X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate (local only).
    /// </summary>
    public double? Y { get; set; }

    /// <summary>
    /// Gets or sets the Z coordinate (local only).
    /// </summary>
    public double? Z { get; set; }
}
