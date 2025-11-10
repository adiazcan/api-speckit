namespace RobotApi.Data;

/// <summary>
/// Base interface for in-memory data stores.
/// </summary>
/// <typeparam name="T">The entity type stored in this data store.</typeparam>
public interface IDataStore<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<T?> UpdateAsync(string id, T entity);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    Task<bool> ExistsAsync(string id);
}
