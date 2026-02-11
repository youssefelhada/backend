namespace visionguard.Models
{
    /// <summary>
    /// Core business entity: PPE Violation Detection
    /// AI model detects violations and posts to backend
    /// Supervisors monitor in real-time, HR uses for historical reporting
    /// </summary>
    public class Violation
    {
        public int Id { get; set; }
        
        /// <summary>
        /// The worker who violated the safety rule
        /// </summary>
        public int WorkerId { get; set; }
        public Worker? Worker { get; set; }
        
        /// <summary>
        /// The camera that detected the violation
        /// Links to specific zone/location for supervisor filtering
        /// </summary>
        public int CameraId { get; set; }
        public Camera? Camera { get; set; }
        
        /// <summary>
        /// Type of PPE violation: Helmet, Vest, Mask, Gloves
        /// Used by HR for breakdown reporting and safety policy analysis
        /// </summary>
        public PPEType ViolationType { get; set; }
        
        /// <summary>
        /// Evidence image from AI detection (blob storage path or URL)
        /// Displayed on violations page for supervisor verification
        /// </summary>
        public string EvidenceImageUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Confidence score from AI model (0-100)
        /// Helps supervisors assess violation authenticity
        /// </summary>
        public int ConfidenceScore { get; set; }
        
        /// <summary>
        /// Timestamp when violation was detected
        /// Critical for filtering (Date From/To) and audit trails
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Supervisor notes (optional) - investigation/context
        /// Justification: Operational documentation for compliance
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Supervision status: Pending, Acknowledged, Resolved
        /// Tracking for dashboard workflow
        /// </summary>
        public ViolationStatus Status { get; set; } = ViolationStatus.PENDING;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// PPE violation types detected by AI model
    /// Each corresponds to safety regulations for different equipment
    /// </summary>
    public enum PPEType
    {
        HELMET,
        VEST,
        MASK,
        GLOVES
    }

    /// <summary>
    /// Violation lifecycle status
    /// Supervisor can acknowledge/resolve for tracking purposes
    /// </summary>
    public enum ViolationStatus
    {
        PENDING,      // New violation, not yet reviewed
        ACKNOWLEDGED, // Supervisor has seen and acknowledged
        RESOLVED      // Incident has been resolved (e.g., worker corrected)
    }
}
