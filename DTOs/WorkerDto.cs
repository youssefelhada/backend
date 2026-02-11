namespace visionguard.DTOs
{
    /// <summary>
    /// RESPONSE: Worker information
    /// Displayed on violations page; read-only for both roles
    /// </summary>
    public class WorkerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Total violations for this worker
        /// Useful for HR disciplinary tracking
        /// </summary>
        public int TotalViolations { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// RESPONSE: Detailed worker profile with violation history
    /// Shown when clicking on worker in violations list
    /// </summary>
    public class WorkerDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        
        /// <summary>
        /// Breakdown of violations by type
        /// HR uses for policy assessment
        /// </summary>
        public ViolationBreakdown ViolationBreakdown { get; set; } = new();
        
        /// <summary>
        /// Recent violations for this worker
        /// Justification: Supervisors track repeat offenders
        /// </summary>
        public List<ViolationDto> RecentViolations { get; set; } = new();
    }

    public class ViolationBreakdown
    {
        public int HelmetViolations { get; set; }
        public int VestViolations { get; set; }
        public int MaskViolations { get; set; }
        public int GlovesViolations { get; set; }
        public int TotalViolations { get; set; }
    }
}
