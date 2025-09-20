using PlaygroundDatabase.Models.Turso;

namespace PlaygroundDatabase.Interfaces;

/// <summary>
/// Interface for database operations
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Executes an SQL query and returns the result
    /// </summary>
    /// <param name="sql">The SQL query to execute</param>
    /// <returns>The Turso response</returns>
    Task<TursoResponse?> ExecuteSqlAsync(string sql);

    /// <summary>
    /// Tests the database connection
    /// </summary>
    /// <returns>True if connection is established</returns>
    Task<bool> TestConnectionAsync();
}
