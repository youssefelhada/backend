using visionguard.DTOs;
using visionguard.Models;
using visionguard.Repositories;

namespace visionguard.Services
{
    /// <summary>
    /// Service layer implementation for Worker operations.
    /// Handles business logic, validation, and coordinates file storage.
    /// </summary>
    public class WorkerService : IWorkerService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(
            IWorkerRepository workerRepository,
            IFileStorageService fileStorageService,
            ILogger<WorkerService> logger)
        {
            _workerRepository = workerRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new worker with photo upload.
        /// Validates uniqueness of WorkerId and saves photo to storage.
        /// </summary>
        public async Task<WorkerResponseDto> CreateWorkerAsync(CreateWorkerDto dto)
        {
            try
            {
                // Validate WorkerId uniqueness
                var exists = await _workerRepository.ExistsByWorkerIdAsync(dto.WorkerId);
                if (exists)
                {
                    throw new InvalidOperationException($"Worker with ID '{dto.WorkerId}' already exists.");
                }

                // Save photo
                var photoUrl = await _fileStorageService.SaveFileAsync(dto.Photo, "workers");

                // Create worker entity
                var worker = new Worker
                {
                    EmployeeId = dto.WorkerId,
                    Name = dto.FullName,
                    ProfilePictureUrl = photoUrl,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database
                await _workerRepository.AddAsync(worker);
                await _workerRepository.SaveChangesAsync();

                _logger.LogInformation("Worker created successfully: {WorkerId}", dto.WorkerId);

                // Return response DTO
                return MapToResponseDto(worker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating worker: {WorkerId}", dto.WorkerId);
                throw;
            }
        }

        /// <summary>
        /// Update an existing worker.
        /// If photo is provided, replaces the old photo.
        /// </summary>
        public async Task<WorkerResponseDto> UpdateWorkerAsync(int id, UpdateWorkerDto dto)
        {
            try
            {
                // Get existing worker
                var worker = await _workerRepository.GetByIdAsync(id);
                if (worker == null)
                {
                    throw new KeyNotFoundException($"Worker with ID {id} not found.");
                }

                // Validate WorkerId uniqueness (excluding current worker)
                var exists = await _workerRepository.ExistsByWorkerIdAsync(dto.WorkerId, id);
                if (exists)
                {
                    throw new InvalidOperationException($"Worker with ID '{dto.WorkerId}' already exists.");
                }

                // Update basic fields
                worker.EmployeeId = dto.WorkerId;
                worker.Name = dto.FullName;

                // Update photo if provided
                if (dto.Photo != null)
                {
                    // Delete old photo
                    if (!string.IsNullOrWhiteSpace(worker.ProfilePictureUrl))
                    {
                        await _fileStorageService.DeleteFileAsync(worker.ProfilePictureUrl);
                    }

                    // Save new photo
                    worker.ProfilePictureUrl = await _fileStorageService.SaveFileAsync(dto.Photo, "workers");
                }

                // Save changes
                _workerRepository.Update(worker);
                await _workerRepository.SaveChangesAsync();

                _logger.LogInformation("Worker updated successfully: {WorkerId}", dto.WorkerId);

                // Return response DTO
                return MapToResponseDto(worker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating worker: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Get all workers.
        /// </summary>
        public async Task<List<WorkerResponseDto>> GetAllWorkersAsync()
        {
            try
            {
                var workers = await _workerRepository.GetAllAsync();
                return workers.Select(MapToResponseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all workers");
                throw;
            }
        }

        /// <summary>
        /// Get a worker by ID.
        /// </summary>
        public async Task<WorkerResponseDto?> GetWorkerByIdAsync(int id)
        {
            try
            {
                var worker = await _workerRepository.GetByIdAsync(id);
                return worker != null ? MapToResponseDto(worker) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving worker: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Delete a worker by ID.
        /// Also deletes the associated photo from storage.
        /// </summary>
        public async Task<bool> DeleteWorkerAsync(int id)
        {
            try
            {
                var worker = await _workerRepository.GetByIdAsync(id);
                if (worker == null)
                {
                    return false;
                }

                // Delete photo from storage
                if (!string.IsNullOrWhiteSpace(worker.ProfilePictureUrl))
                {
                    await _fileStorageService.DeleteFileAsync(worker.ProfilePictureUrl);
                }

                // Delete worker from database
                _workerRepository.Remove(worker);
                await _workerRepository.SaveChangesAsync();

                _logger.LogInformation("Worker deleted successfully: {Id}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting worker: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Map Worker entity to WorkerResponseDto.
        /// </summary>
        private WorkerResponseDto MapToResponseDto(Worker worker)
        {
            return new WorkerResponseDto
            {
                Id = worker.Id,
                WorkerId = worker.EmployeeId,
                FullName = worker.Name,
                PhotoUrl = worker.ProfilePictureUrl ?? string.Empty,
                CreatedAt = worker.CreatedAt
            };
        }
    }
}
