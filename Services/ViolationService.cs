using Microsoft.EntityFrameworkCore;
using visionguard.Data;
using visionguard.DTOs;
using visionguard.Models;

namespace visionguard.Services
{
    public class ViolationService : IViolationService
    {
        private readonly VisionGuardDbContext _context;

        public ViolationService(VisionGuardDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<ViolationDto>> GetViolationsAsync(ViolationFilterRequest filter)
        {
            var query = _context.Violations
                .Include(v => v.Worker)
                .Include(v => v.Camera)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrEmpty(filter.ViolationType))
            {
                if (Enum.TryParse<PPEType>(filter.ViolationType, true, out var type))
                    query = query.Where(v => v.ViolationType == type);
            }

            if (!string.IsNullOrEmpty(filter.EmployeeId))
            {
                query = query.Where(v => v.Worker.EmployeeId == filter.EmployeeId);
            }
            if (filter.DateFrom.HasValue)
                query = query.Where(v => v.DetectedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(v => v.DetectedAt <= filter.DateTo.Value.AddDays(1)); // Inclusive of end date

            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (Enum.TryParse<ViolationStatus>(filter.Status, true, out var status))
                    query = query.Where(v => v.Status == status);
            }

            // Sorting
            query = filter.SortBy.ToLower() switch
            {
                "workerid" => filter.SortOrder.ToUpper() == "DESC" ? query.OrderByDescending(v => v.WorkerId) : query.OrderBy(v => v.WorkerId),
                "camerazone" => filter.SortOrder.ToUpper() == "DESC" ? query.OrderByDescending(v => v.Camera!.Zone) : query.OrderBy(v => v.Camera!.Zone),
                _ => filter.SortOrder.ToUpper() == "DESC" ? query.OrderByDescending(v => v.DetectedAt) : query.OrderBy(v => v.DetectedAt), // Default DetectedAt
            };

            // Pagination
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new ViolationDto
                {
                    Id = v.Id,
                    WorkerId = v.WorkerId,
                    WorkerName = v.Worker!.Name,
                    WorkerEmployeeId = v.Worker.EmployeeId,
                    CameraId = v.CameraId,
                    CameraZone = v.Camera!.Zone,
                    CameraDescription = v.Camera!.Description ?? string.Empty,
                    ViolationType = v.ViolationType.ToString(),
                    Status = v.Status.ToString(),
                    EvidenceImageUrl = v.EvidenceImageUrl,
                    ConfidenceScore = v.ConfidenceScore,
                    DetectedAt = v.DetectedAt,
                    Notes = v.Notes
                })
                .ToListAsync();

            return new PagedResponse<ViolationDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<ViolationDto?> GetViolationByIdAsync(int id)
        {
            var v = await _context.Violations
                .Include(v => v.Worker)
                .Include(v => v.Camera)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (v == null) return null;

            return new ViolationDto
            {
                Id = v.Id,
                WorkerId = v.WorkerId,
                WorkerName = v.Worker!.Name,
                WorkerEmployeeId = v.Worker.EmployeeId,
                CameraId = v.CameraId,
                CameraZone = v.Camera!.Zone,
                CameraDescription = v.Camera!.Description ?? string.Empty,
                ViolationType = v.ViolationType.ToString(),
                Status = v.Status.ToString(),
                EvidenceImageUrl = v.EvidenceImageUrl,
                ConfidenceScore = v.ConfidenceScore,
                DetectedAt = v.DetectedAt,
                Notes = v.Notes
            };
        }

        public async Task<ViolationDto> CreateViolationAsync(CreateViolationRequest request)
        {
            // Validate foreign keys (simplified)
            var workerExists = await _context.Workers.AnyAsync(w => w.Id == request.WorkerId);
            var cameraExists = await _context.Cameras.AnyAsync(c => c.Id == request.CameraId);

            if (!workerExists) throw new Exception("Worker not found");
            if (!cameraExists) throw new Exception("Camera not found");

            Enum.TryParse<PPEType>(request.ViolationType, true, out var type);

            var violation = new Violation
            {
                WorkerId = request.WorkerId,
                CameraId = request.CameraId,
                ViolationType = type,
                EvidenceImageUrl = request.EvidenceImageUrl,
                ConfidenceScore = request.ConfidenceScore,
                DetectedAt = request.DetectedAt,
                Status = ViolationStatus.PENDING
            };

            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();

            // Fetch back to return DTO with joins
            return await GetViolationByIdAsync(violation.Id) ?? throw new Exception("Error retrieving created violation");
        }

        public async Task<ViolationDto> UpdateViolationAsync(int id, UpdateViolationRequest request)
        {
            var violation = await _context.Violations.FindAsync(id);
            if (violation == null) throw new Exception("Violation not found");

            if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<ViolationStatus>(request.Status, true, out var status))
            {
                violation.Status = status;
            }

            if (request.Notes != null)
            {
                violation.Notes = request.Notes;
            }

            await _context.SaveChangesAsync();

            return await GetViolationByIdAsync(id) ?? throw new Exception("Error retrieving updated violation");
        }

        public async Task<object> GetDashboardStatisticsAsync()
        {
            // Simple statistics
            var today = DateTime.UtcNow.Date;

            var totalToday = await _context.Violations.CountAsync(v => v.DetectedAt >= today);

            var byType = await _context.Violations
                .GroupBy(v => v.ViolationType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            var pendingCount = await _context.Violations.CountAsync(v => v.Status == ViolationStatus.PENDING);

            return new
            {
                TotalViolationsToday = totalToday,
                PendingReviews = pendingCount,
                ViolationsByType = byType
            };
        }
    }
}
