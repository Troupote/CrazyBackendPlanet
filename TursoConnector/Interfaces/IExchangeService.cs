using TursoConnector.Models.Business;

namespace TursoConnector.Interfaces;

/// <summary>
/// Interface for exchange management service
/// </summary>
public interface IExchangeService
{
    /// <summary>
    /// Creates the exchanges table if it doesn't exist
    /// </summary>
    Task<bool> CreateExchangeTableAsync();

    /// <summary>
    /// Inserts a new exchange into the database
    /// </summary>
    /// <param name="exchange">The exchange to insert</param>
    Task<bool> InsertExchangeAsync(Exchange exchange);

    /// <summary>
    /// Retrieves all exchanges
    /// </summary>
    /// <returns>List of exchanges</returns>
    Task<List<Exchange>> GetAllExchangesAsync();

    /// <summary>
    /// Retrieves an exchange by its ID
    /// </summary>
    /// <param name="id">The exchange ID</param>
    /// <returns>The found exchange or null</returns>
    Task<Exchange?> GetExchangeByIdAsync(int id);
}
