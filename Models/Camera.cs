namespace visionguard.Models
{
    /// <summary>
    /// Represents a physical camera location in the factory
    /// Cameras are zone-based for supervisor monitoring
    /// Violations are traced back to specific cameras
    /// </summary>
    public class Camera
    {
        public int Id { get; set; }
        public string CameraId { get; set; } = string.Empty;  // Physical camera identifier (e.g., "CAM-001")
        
        /// <summary>
        /// Zone/location name (e.g., "Assembly Line A", "Welding Station B")
        /// Used for supervisor filtering and area management
        /// </summary>
        public string Zone { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    }
}
