namespace visionguard.DTOs
{
    /// <summary>
    /// RESPONSE: Violation displayed on Violations page
    /// Contains all info needed for supervisor/HR dashboard visualization
    /// </summary>
    public class ViolationDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Worker information - displayed on dashboard
        /// </summary>
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string WorkerEmployeeId { get; set; } = string.Empty;

        /// <summary>
        /// Camera/Zone information - used for supervisor filtering
        /// </summary>
        public int CameraId { get; set; }
        public string CameraZone { get; set; } = string.Empty;
        public string CameraDescription { get; set; } = string.Empty;

        /// <summary>
        /// Violation details
        /// ViolationType is filter option on frontend (Helmet, Vest, Mask, Gloves)
        /// </summary>
        public string ViolationType { get; set; } = string.Empty;  // "HELMET", "VEST", etc.
        public string Status { get; set; } = string.Empty;         // "PENDING", "ACKNOWLEDGED", etc.

        /// <summary>
        /// Evidence proof image
        /// Displayed on violations page for visual confirmation
        /// </summary>
        public string EvidenceImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// AI confidence score - helps supervisor trust the detection
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// When the violation occurred
        /// Critical for date filtering (Date From/To)
        /// </summary>
        public DateTime DetectedAt { get; set; }

        /// <summary>
        /// Supervisor notes for investigation tracking
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// REQUEST: Create new violation (posted by AI service)
    /// </summary>
    public class CreateViolationRequest
    {
        public int WorkerId { get; set; }
        public int CameraId { get; set; }
        public string ViolationType { get; set; } = string.Empty;  // "HELMET", "VEST", "MASK", "GLOVES"
        public string EvidenceImageUrl { get; set; } = string.Empty;
        public int ConfidenceScore { get; set; }
        public DateTime DetectedAt { get; set; }
    }

    /// <summary>
    /// REQUEST: Update violation status or notes
    /// Supervisor can acknowledge/resolve violations
    /// </summary>
    public class UpdateViolationRequest
    {
        public string? Status { get; set; }  // Optional: update status
        public string? Notes { get; set; }   // Optional: add supervisor notes
    }

    /// <summary>
    /// REQUEST: Filter/search violations on dashboard
    /// Justification: Violations page requires multiple filters for supervisor efficiency
    /// </summary>
    public class ViolationFilterRequest
    {
        /// <summary>
        /// Filter by camera zone (e.g., "Assembly Line A")
        /// Supervisors monitor specific areas
        /// </summary>
        public string? CameraZone { get; set; }

        /// <summary>
        /// Filter by violation type (e.g., "HELMET", "VEST")
        /// HR may focus on specific PPE problems
        /// </summary>
        public string? ViolationType { get; set; }

        /// <summary>
        /// Filter by worker ID
        /// Supervisors investigate repeated offenders
        /// </summary>
        public string? EmployeeId { get; set; }

        /// <summary>
        /// Date range filter - critical for audits
        /// HR pulls monthly reports, supervisors check recent hours
        /// </summary>
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Filter by status (PENDING, ACKNOWLEDGED, RESOLVED)
        /// Supervisors track workflow
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Pagination support
        /// </summary>
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Sort order: newest first, oldest first
        /// </summary>
        public string SortBy { get; set; } = "DetectedAt";  // "DetectedAt", "WorkerId", "CameraZone"
        public string SortOrder { get; set; } = "DESC";     // "ASC" or "DESC"
    }
}
