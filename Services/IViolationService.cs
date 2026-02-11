using visionguard.DTOs;

namespace visionguard.Services
{
    public interface IViolationService
    {
        Task<PagedResponse<ViolationDto>> GetViolationsAsync(ViolationFilterRequest filter);
        Task<ViolationDto?> GetViolationByIdAsync(int id);
        Task<ViolationDto> CreateViolationAsync(CreateViolationRequest request);
        Task<ViolationDto> UpdateViolationAsync(int id, UpdateViolationRequest request);
        Task<object> GetDashboardStatisticsAsync();
    }
}
