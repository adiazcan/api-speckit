namespace RobotApi.Models;

/// <summary>
/// Represents robot orientation in 3D space.
/// </summary>
public sealed class Orientation
{
    /// <summary>
    /// Gets or sets the pitch angle in degrees.
    /// Rotation around the lateral axis (nose up/down).
    /// </summary>
    public required double Pitch { get; set; }

    /// <summary>
    /// Gets or sets the roll angle in degrees.
    /// Rotation around the longitudinal axis (tilting left/right).
    /// </summary>
    public required double Roll { get; set; }

    /// <summary>
    /// Gets or sets the yaw angle in degrees.
    /// Rotation around the vertical axis (turning left/right).
    /// </summary>
    public required double Yaw { get; set; }
}
