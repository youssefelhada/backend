using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visionguard.Data;
using visionguard.DTOs;
using visionguard.Models;

namespace visionguard.Controllers
{
    /// <summary>
    /// Cameras Controller — manages camera locations and zones
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CamerasController : ControllerBase
    {
        private readonly VisionGuardDbContext _context;

        public CamerasController(VisionGuardDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/cameras — List all cameras with violation counts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCameras()
        {
            var cameras = await _context.Cameras
                .Select(c => new CameraDto
                {
                    Id = c.Id,
                    CameraId = c.CameraId,
                    Zone = c.Zone,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    TotalViolations = c.Violations.Count,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .OrderBy(c => c.Zone)
                .ToListAsync();

            return Ok(new ApiResponse<List<CameraDto>>
            {
                Success = true,
                Data = cameras
            });
        }

        /// <summary>
        /// GET /api/cameras/{id} — Get single camera details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCamera(int id)
        {
            var camera = await _context.Cameras
                .Where(c => c.Id == id)
                .Select(c => new CameraDto
                {
                    Id = c.Id,
                    CameraId = c.CameraId,
                    Zone = c.Zone,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    TotalViolations = c.Violations.Count,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (camera == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Camera not found" });

            return Ok(new ApiResponse<CameraDto> { Success = true, Data = camera });
        }

        /// <summary>
        /// POST /api/cameras — Create new camera (Supervisor only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SAFETY_SUPERVISOR")]
        public async Task<IActionResult> CreateCamera([FromBody] CreateUpdateCameraRequest request)
        {
            if (await _context.Cameras.AnyAsync(c => c.CameraId == request.CameraId))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Camera ID already exists" });

            var camera = new Camera
            {
                CameraId = request.CameraId,
                Zone = request.Zone,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();

            var dto = new CameraDto
            {
                Id = camera.Id,
                CameraId = camera.CameraId,
                Zone = camera.Zone,
                Description = camera.Description,
                IsActive = camera.IsActive,
                TotalViolations = 0,
                CreatedAt = camera.CreatedAt
            };

            return CreatedAtAction(nameof(GetCamera), new { id = camera.Id },
                new ApiResponse<CameraDto> { Success = true, Message = "Camera created successfully", Data = dto });
        }

        /// <summary>
        /// PUT /api/cameras/{id} — Update camera (Supervisor only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SAFETY_SUPERVISOR")]
        public async Task<IActionResult> UpdateCamera(int id, [FromBody] CreateUpdateCameraRequest request)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Camera not found" });

            camera.Zone = request.Zone;
            camera.Description = request.Description;
            camera.IsActive = request.IsActive;
            camera.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = new CameraDto
            {
                Id = camera.Id,
                CameraId = camera.CameraId,
                Zone = camera.Zone,
                Description = camera.Description,
                IsActive = camera.IsActive,
                TotalViolations = await _context.Violations.CountAsync(v => v.CameraId == id),
                CreatedAt = camera.CreatedAt,
                UpdatedAt = camera.UpdatedAt
            };

            return Ok(new ApiResponse<CameraDto> { Success = true, Message = "Camera updated successfully", Data = dto });
        }

        /// <summary>
        /// DELETE /api/cameras/{id} — Soft-delete camera (Supervisor only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SAFETY_SUPERVISOR")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Camera not found" });

            camera.IsActive = false;
            camera.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { Success = true, Message = "Camera deactivated successfully" });
        }

        /// <summary>
        /// GET /api/cameras/{id}/violations — Paginated violations from a camera
        /// </summary>
        [HttpGet("{id}/violations")]
        public async Task<IActionResult> GetCameraViolations(int id,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var query = _context.Violations
                .Include(v => v.Worker)
                .Include(v => v.Camera)
                .Where(v => v.CameraId == id)
                .OrderByDescending(v => v.DetectedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new ViolationDto
                {
                    Id = v.Id,
                    WorkerId = v.WorkerId,
                    WorkerName = v.Worker!.Name,
                    WorkerEmployeeId = v.Worker.EmployeeId,
                    CameraId = v.CameraId,
                    CameraZone = v.Camera!.Zone,
                    CameraDescription = v.Camera.Description ?? "",
                    ViolationType = v.ViolationType.ToString(),
                    Status = v.Status.ToString(),
                    EvidenceImageUrl = v.EvidenceImageUrl,
                    ConfidenceScore = v.ConfidenceScore,
                    DetectedAt = v.DetectedAt,
                    Notes = v.Notes
                })
                .ToListAsync();

            return Ok(new PagedResponse<ViolationDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        }
    }
}
