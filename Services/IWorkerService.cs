using visionguard.DTOs;

namespace visionguard.Services
{
    /// <summary>
    /// Service layer abstraction for Worker operations.
    /// Encapsulates business logic and coordinates between controller and repository.
    /// </summary>
    public interface IWorkerService
    {
        /// <summary>
        /// Create a new worker with photo upload.
        /// </summary>
        /// <param name="dto">Worker creation data with photo.</param>
        /// <returns>Created worker response.</returns>
        Task<WorkerResponseDto> CreateWorkerAsync(CreateWorkerDto dto);

        /// <summary>
        /// Update an existing worker.
        /// </summary>
        /// <param name="id">Worker database ID.</param>
        /// <param name="dto">Updated worker data (photo optional).</param>
        /// <returns>Updated worker response.</returns>
        Task<WorkerResponseDto> UpdateWorkerAsync(int id, UpdateWorkerDto dto);

        /// <summary>
        /// Get all workers.
        /// </summary>
        /// <returns>List of all workers.</returns>
        Task<List<WorkerResponseDto>> GetAllWorkersAsync();

        /// <summary>
        /// Get a worker by ID.
        /// </summary>
        /// <param name="id">Worker database ID.</param>
        /// <returns>Worker response or null if not found.</returns>
        Task<WorkerResponseDto?> GetWorkerByIdAsync(int id);

        /// <summary>
        /// Delete a worker by ID.
        /// </summary>
        /// <param name="id">Worker database ID.</param>
        /// <returns>True if deleted successfully, false if not found.</returns>
        Task<bool> DeleteWorkerAsync(int id);
    }
}
