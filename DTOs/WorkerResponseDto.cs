namespace visionguard.DTOs
{
    /// <summary>
    /// RESPONSE: Worker information shared across React, Flutter, and AI clients.
    /// Intentionally minimal: no roles, departments, or authentication details.
    /// </summary>
    public class WorkerResponseDto
    {
        /// <summary>
        /// Database identifier (primary key).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Human-friendly worker identifier (e.g., EMP-001).
        /// </summary>
        public string WorkerId { get; set; } = string.Empty;

        /// <summary>
        /// Full display name of the worker.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Public/relative URL to the stored worker photo.
        /// Example: /uploads/workers/abc123.jpg
        /// </summary>
        public string PhotoUrl { get; set; } = string.Empty;

        /// <summary>
        /// When the worker record was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}

