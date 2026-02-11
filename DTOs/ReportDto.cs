namespace visionguard.DTOs
{
    /// <summary>
    /// RESPONSE: Worker violation report with per-type breakdown
    /// Matches frontend WorkerViolationReportDto interface
    /// </summary>
    public class WorkerViolationReportDto
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public int TotalViolations { get; set; }
        public List<ViolationTypeCountDto> Breakdown { get; set; } = new();
    }

    /// <summary>
    /// RESPONSE: Violation count by type
    /// Matches frontend ViolationTypeCount interface
    /// </summary>
    public class ViolationTypeCountDto
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// RESPONSE: Monthly summary
    /// Matches frontend MonthlySummaryDto interface
    /// </summary>
    public class MonthlySummaryResponseDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalViolations { get; set; }
        public List<ViolationTypeCountDto> Breakdown { get; set; } = new();
    }

    /// <summary>
    /// REQUEST: Report generation filter
    /// </summary>
    public class ReportFilterRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? CameraZone { get; set; }
        public string? ViolationType { get; set; }
    }

    /// <summary>
    /// RESPONSE: Export-ready violation data
    /// </summary>
    public class ExportViolationDto
    {
        public int ViolationId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string WorkerEmployeeId { get; set; } = string.Empty;
        public string CameraZone { get; set; } = string.Empty;
        public string ViolationType { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public int ConfidenceScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
