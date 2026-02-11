using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace visionguard.DTOs
{
    /// <summary>
    /// REQUEST: Create a new factory worker.
    /// Used by admin React app, Flutter mobile app, and AI tooling.
    /// </summary>
    public class CreateWorkerDto
    {
        /// <summary>
        /// Human-friendly worker identifier (e.g., EMP-001).
        /// Must be unique across all workers.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string WorkerId { get; set; } = string.Empty;

        /// <summary>
        /// Full display name of the worker.
        /// </summary>
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Worker profile photo uploaded as multipart/form-data.
        /// Stored in local storage; only the URL/path is persisted in DB.
        /// </summary>
        [Required]
        public IFormFile Photo { get; set; } = null!;
    }
}

