using RobotApi.Models;

namespace RobotApi.Data;

/// <summary>
/// Interface for Robot data store with specialized methods.
/// </summary>
public interface IRobotStore : IDataStore<Robot>
{
    /// <summary>
    /// Updates the last seen timestamp for a robot (used by telemetry updates).
    /// </summary>
    Task UpdateLastSeenAsync(string id);
}
