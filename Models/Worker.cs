namespace visionguard.Models
{
    /// <summary>
    /// Represents a factory worker being monitored by AI.
    /// Workers do NOT login to the system.
    /// They only receive violations.
    /// Used by: React frontend, Flutter mobile app, AI model.
    /// </summary>
    public class Worker
    {
        /// <summary>
        /// Database primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique worker identifier (e.g., EMP-001).
        /// Stored as EmployeeId in database for backward compatibility.
        /// </summary>
        public string EmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// Full name of the worker.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Stored image path/URL for worker photo.
        /// Example: /uploads/workers/abc123.jpg
        /// </summary>
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// When the worker record was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
    }
}
