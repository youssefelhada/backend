using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using visionguard.DTOs;
using visionguard.Services;

namespace visionguard.Controllers
{
    /// <summary>
    /// Violations Controller - CORE SYSTEM ENDPOINT
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ViolationsController : ControllerBase
    {
        private readonly IViolationService _violationService;

        public ViolationsController(IViolationService violationService)
        {
            _violationService = violationService;
        }

        /// <summary>
        /// GET /api/violations
        /// Search and filter violations
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> GetViolations([FromBody] ViolationFilterRequest filter)
        {
            var result = await _violationService.GetViolationsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/violations/{id}
        /// Get detailed violation info
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetViolationDetail(int id)
        {
            var violation = await _violationService.GetViolationByIdAsync(id);
            if (violation == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "Violation not found" });
            }

            return Ok(new ApiResponse<ViolationDto>
            {
                Success = true,
                Data = violation
            });
        }

        /// <summary>
        /// POST /api/violations
        /// AI Detection Endpoint
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // Should be protected by API Key in production
        public async Task<IActionResult> CreateViolation([FromBody] CreateViolationRequest request)
        {
            try
            {
                var violation = await _violationService.CreateViolationAsync(request);
                return CreatedAtAction(nameof(GetViolationDetail), new { id = violation.Id },
                    new ApiResponse<ViolationDto>
                    {
                        Success = true,
                        Message = "Violation recorded successfully",
                        Data = violation
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// PUT /api/violations/{id}
        /// Update status or notes
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "SupervisorOnly")]
        public async Task<IActionResult> UpdateViolation(int id, [FromBody] UpdateViolationRequest request)
        {
            try
            {
                var violation = await _violationService.UpdateViolationAsync(id, request);
                return Ok(new ApiResponse<ViolationDto>
                {
                    Success = true,
                    Message = "Violation updated successfully",
                    Data = violation
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/violations/statistics/dashboard
        /// Dashboard KPIs
        /// </summary>
        [HttpGet("statistics/dashboard")]
        public async Task<IActionResult> GetDashboardStatistics()
        {
            var stats = await _violationService.GetDashboardStatisticsAsync();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = stats
            });
        }
    }
}
