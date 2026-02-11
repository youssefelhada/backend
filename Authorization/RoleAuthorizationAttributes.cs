using Microsoft.AspNetCore.Authorization;

namespace visionguard.Authorization
{
    /// <summary>
    /// Custom authorization attributes for role-based access control
    /// Justification: Different dashboard features require different roles
    /// </summary>

    /// <summary>
    /// Requires user to be SAFETY_SUPERVISOR
    /// Justification: Cameras, users, and operational control are supervisor-only
    /// </summary>
    public class RequireSupervisorAttribute : AuthorizeAttribute
    {
        public RequireSupervisorAttribute() => Roles = "SAFETY_SUPERVISOR";
    }

    /// <summary>
    /// Requires user to be HR
    /// Justification: Reporting and compliance features are HR-only
    /// </summary>
    public class RequireHRAttribute : AuthorizeAttribute
    {
        public RequireHRAttribute() => Roles = "HR";
    }

    /// <summary>
    /// Allows both SAFETY_SUPERVISOR and HR
    /// Justification: Violations page is shared; filters differ by role
    /// </summary>
    public class RequireAuthenticatedAttribute : AuthorizeAttribute
    {
        // No specific role - just requires authentication
    }
}
