namespace RobotApi.Models;

/// <summary>
/// Base interface for all entities in the system.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    string Id { get; set; }
}
