namespace visionguard.DTOs
{
    /// <summary>
    /// RESPONSE: Camera with violation count
    /// Displayed on Camera Locations page for supervisor
    /// Justification: Helps identify high-violation areas
    /// </summary>
    public class CameraDto
    {
        public int Id { get; set; }
        public string CameraId { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Total violations detected by this camera
        /// Performance indicator for camera effectiveness
        /// </summary>
        public int TotalViolations { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// REQUEST: Create or update camera
    /// Only Safety Supervisor can manage cameras
    /// </summary>
    public class CreateUpdateCameraRequest
    {
        public string CameraId { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
