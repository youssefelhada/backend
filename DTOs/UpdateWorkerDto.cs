using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace visionguard.DTOs
{
    /// <summary>
    /// REQUEST: Update an existing factory worker.
    /// Photo is optional; if provided, it will replace the existing image.
    /// </summary>
    public class UpdateWorkerDto
    {
        /// <summary>
        /// Human-friendly worker identifier (e.g., EMP-001).
        /// Must remain unique across all workers.
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
        /// New profile photo. Optional.
        /// If null, the existing photo is preserved.
        /// </summary>
        public IFormFile? Photo { get; set; }
    }
}

