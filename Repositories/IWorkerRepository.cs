using visionguard.Models;

namespace visionguard.Repositories
{
    /// <summary>
    /// Repository abstraction for factory workers.
    /// Encapsulates all data access for the Worker aggregate.
    /// </summary>
    public interface IWorkerRepository
    {
        /// <summary>
        /// Get a worker by database Id (primary key).
        /// </summary>
        Task<Worker?> GetByIdAsync(int id);

        /// <summary>
        /// Get a worker by business WorkerId (e.g., EMP-001).
        /// </summary>
        Task<Worker?> GetByWorkerIdAsync(string workerId);

        /// <summary>
        /// Get all workers.
        /// For large datasets, prefer adding pagination at the service/controller level.
        /// </summary>
        Task<List<Worker>> GetAllAsync();

        /// <summary>
        /// Add a new worker to the context.
        /// Does not persist until SaveChangesAsync is called.
        /// </summary>
        Task AddAsync(Worker worker);

        /// <summary>
        /// Mark an existing worker as modified.
        /// Does not persist until SaveChangesAsync is called.
        /// </summary>
        void Update(Worker worker);

        /// <summary>
        /// Remove an existing worker.
        /// Does not persist until SaveChangesAsync is called.
        /// </summary>
        void Remove(Worker worker);

        /// <summary>
        /// Check if a worker with the given WorkerId exists.
        /// Optional excludeId allows uniqueness checks on update.
        /// </summary>
        Task<bool> ExistsByWorkerIdAsync(string workerId, int? excludeId = null);

        /// <summary>
        /// Persist pending changes to the database.
        /// </summary>
        Task SaveChangesAsync();
    }
}

