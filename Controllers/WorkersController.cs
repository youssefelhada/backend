using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visionguard.DTOs;
using visionguard.Services;

namespace visionguard.Controllers
{
    /// <summary>
    /// Workers Controller - Manages factory workers (NOT system users).
    /// Workers are detected by AI and receive violations.
    /// Used by: React frontend, Flutter mobile app, AI model.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkersController : ControllerBase
    {
        private readonly IWorkerService _workerService;
        private readonly ILogger<WorkersController> _logger;

        public WorkersController(
            IWorkerService workerService,
            ILogger<WorkersController> logger)
        {
            _workerService = workerService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new worker with photo upload.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/workers
        ///     Content-Type: multipart/form-data
        ///     
        ///     workerId: EMP-001
        ///     fullName: John Doe
        ///     photo: [binary file]
        /// 
        /// </remarks>
        /// <param name="dto">Worker creation data with photo.</param>
        /// <returns>Created worker.</returns>
        /// <response code="201">Worker created successfully.</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="409">Worker with the same WorkerId already exists.</response>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(WorkerResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateWorker([FromForm] CreateWorkerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var worker = await _workerService.CreateWorkerAsync(dto);

                return CreatedAtAction(
                    nameof(GetWorkerById),
                    new { id = worker.Id },
                    worker);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Worker creation failed: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file upload: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating worker");
                return StatusCode(500, new { message = "An error occurred while creating the worker." });
            }
        }

        /// <summary>
        /// Update an existing worker.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/workers/1
        ///     Content-Type: multipart/form-data
        ///     
        ///     workerId: EMP-001
        ///     fullName: John Doe Updated
        ///     photo: [binary file] (optional)
        /// 
        /// </remarks>
        /// <param name="id">Worker database ID.</param>
        /// <param name="dto">Updated worker data.</param>
        /// <returns>Updated worker.</returns>
        /// <response code="200">Worker updated successfully.</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="404">Worker not found.</response>
        /// <response code="409">Worker with the same WorkerId already exists.</response>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(WorkerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateWorker(int id, [FromForm] UpdateWorkerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var worker = await _workerService.UpdateWorkerAsync(id, dto);

                return Ok(worker);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Worker not found: {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Worker update failed: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid file upload: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating worker: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the worker." });
            }
        }

        /// <summary>
        /// Get all workers.
        /// </summary>
        /// <returns>List of all workers.</returns>
        /// <response code="200">Returns the list of workers.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<WorkerResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllWorkers()
        {
            try
            {
                var workers = await _workerService.GetAllWorkersAsync();
                return Ok(workers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving workers");
                return StatusCode(500, new { message = "An error occurred while retrieving workers." });
            }
        }

        /// <summary>
        /// Get a worker by ID.
        /// </summary>
        /// <param name="id">Worker database ID.</param>
        /// <returns>Worker details.</returns>
        /// <response code="200">Returns the worker.</response>
        /// <response code="404">Worker not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(WorkerResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkerById(int id)
        {
            try
            {
                var worker = await _workerService.GetWorkerByIdAsync(id);

                if (worker == null)
                {
                    return NotFound(new { message = $"Worker with ID {id} not found." });
                }

                return Ok(worker);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving worker: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the worker." });
            }
        }

        /// <summary>
        /// Delete a worker by ID.
        /// </summary>
        /// <param name="id">Worker database ID.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Worker deleted successfully.</response>
        /// <response code="404">Worker not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWorker(int id)
        {
            try
            {
                var deleted = await _workerService.DeleteWorkerAsync(id);

                if (!deleted)
                {
                    return NotFound(new { message = $"Worker with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting worker: {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the worker." });
            }
        }
    }
}
