/**
 * ============================================================================
 * VISION GUARD - API ARCHITECTURE DOCUMENTATION
 * ============================================================================
 * 
 * PROJECT OVERVIEW
 * ============================================================================
 * Vision Guard is an industrial safety monitoring system that:
 * - Uses AI to detect PPE (Personal Protective Equipment) violations
 * - Alerts supervisors to safety incidents in real-time
 * - Provides HR with compliance data for reporting and audits
 * - Maintains audit trail for regulatory compliance
 * 
 * SYSTEM USERS
 * ============================================================================
 * 1. SAFETY_SUPERVISOR
 *    - Real-time safety monitoring
 *    - Operational control (cameras, users)
 *    - Can acknowledge/resolve violations
 *    - CAN manage cameras and system users
 * 
 * 2. HR
 *    - Historical data analysis
 *    - Compliance reporting
 *    - Export to Excel/PDF for audits
 *    - CANNOT manage cameras or users
 * 
 * NOTE: Factory workers are NOT users - they are monitored subjects
 * 
 * ============================================================================
 * API ENDPOINTS BY FEATURE
 * ============================================================================
 * 
 * AUTHENTICATION (Public)
 * ============================================================================
 * POST   /api/auth/login               ? Authenticate user, get JWT token
 * GET    /api/auth/profile             ? Get current user profile
 * PUT    /api/auth/profile             ? Update current user profile
 * PUT    /api/auth/change-password     ? Change password
 * POST   /api/auth/refresh             ? Refresh JWT token
 * POST   /api/auth/logout              ? Logout (optional, client-side mainly)
 * 
 * 
 * VIOLATIONS (Core Business Entity)
 * ============================================================================
 * JUSTIFICATION:
 * - Violations are the PRIMARY data entity of Vision Guard
 * - Supervisors monitor violations in real-time (GET with filters)
 * - AI service posts new violations (POST from edge devices)
 * - HR analyzes violations for compliance reporting
 * 
 * ENDPOINTS:
 * POST   /api/violations                     ? AI posts new PPE violation
 * POST   /api/violations/search              ? Get violations with filters & pagination
 *        FILTERS: cameraZone, violationType, workerId, dateFrom, dateTo, status
 *        PAGINATION: pageNumber, pageSize
 *        SORTING: sortBy (DetectedAt, WorkerId, CameraZone), sortOrder (ASC/DESC)
 * 
 * GET    /api/violations/{id}               ? Get detailed violation info
 * PUT    /api/violations/{id}               ? Update violation status/notes
 * GET    /api/violations/statistics/dashboard ? Dashboard summary stats
 * 
 * RESPONSE SHAPE (ViolationDto):
 * {
 *   "id": 1,
 *   "workerId": 101,
 *   "workerName": "John Doe",
 *   "workerEmployeeId": "EMP001",
 *   "cameraId": 5,
 *   "cameraZone": "Assembly Line A",
 *   "violationType": "HELMET",
 *   "status": "PENDING",
 *   "evidenceImageUrl": "https://blob.../violation_img.jpg",
 *   "confidenceScore": 95,
 *   "detectedAt": "2024-01-15T14:32:00Z",
 *   "notes": null
 * }
 * 
 * 
 * CAMERAS (Supervisor Only)
 * ============================================================================
 * JUSTIFICATION:
 * - Physical assets in the factory
 * - Violations are traced to specific cameras
 * - Supervisors manage camera locations and zones for area-based monitoring
 * - HR cannot manage cameras (security boundary)
 * 
 * ENDPOINTS (SUPERVISOR ONLY):
 * GET    /api/cameras                  ? List all cameras with violation counts
 * POST   /api/cameras                  ? Create new camera
 * GET    /api/cameras/{id}             ? Get camera details
 * PUT    /api/cameras/{id}             ? Update camera info
 * DELETE /api/cameras/{id}             ? Delete camera (soft delete recommended)
 * GET    /api/cameras/{id}/violations  ? Get all violations from camera
 * 
 * RESPONSE SHAPE (CameraDto):
 * {
 *   "id": 1,
 *   "cameraId": "CAM-001",
 *   "zone": "Assembly Line A",
 *   "description": "Main entrance to assembly",
 *   "isActive": true,
 *   "totalViolations": 42,
 *   "createdAt": "2024-01-01T10:00:00Z",
 *   "updatedAt": "2024-01-10T14:32:00Z"
 * }
 * 
 * 
 * USERS (Supervisor Only)
 * ============================================================================
 * JUSTIFICATION:
 * - System access control
 * - Supervisors create/manage accounts for team members
 * - HR cannot manage user accounts
 * 
 * ENDPOINTS (SUPERVISOR ONLY):
 * GET    /api/users                   ? List all system users
 * POST   /api/users                   ? Create new user account
 * GET    /api/users/{id}              ? Get user details
 * PUT    /api/users/{id}              ? Update user info
 * DELETE /api/users/{id}              ? Delete user (soft delete)
 * POST   /api/users/{id}/reset-password ? Reset forgotten password
 * 
 * RESPONSE SHAPE (UserDto):
 * {
 *   "id": 1,
 *   "username": "supervisor1",
 *   "firstName": "John",
 *   "lastName": "Smith",
 *   "email": "john@company.com",
 *   "employeeId": "EMP001",
 *   "department": "Safety",
 *   "role": "SAFETY_SUPERVISOR",
 *   "isActive": true,
 *   "createdAt": "2024-01-01T10:00:00Z",
 *   "lastLoginAt": "2024-01-15T14:32:00Z"
 * }
 * 
 * 
 * WORKERS (Read-Only, Both Roles)
 * ============================================================================
 * JUSTIFICATION:
 * - Factory workers are monitored subjects (NOT system users)
 * - Both supervisors and HR need to view worker info for violations
 * - Workers managed by HR systems (integration sync), not API
 * 
 * ENDPOINTS (READ-ONLY):
 * GET    /api/workers                  ? List all workers with violation counts
 * GET    /api/workers/{id}             ? Get worker profile & violation breakdown
 * GET    /api/workers/{id}/violations  ? Get violations for specific worker
 * GET    /api/workers/search           ? Search workers by name or employee ID
 * 
 * RESPONSE SHAPE (WorkerDto):
 * {
 *   "id": 1,
 *   "name": "John Doe",
 *   "employeeId": "EMP001",
 *   "department": "Assembly",
 *   "profilePictureUrl": "https://blob.../worker1.jpg",
 *   "isActive": true,
 *   "totalViolations": 3,
 *   "createdAt": "2023-12-01T10:00:00Z"
 * }
 * 
 * WORKER DETAIL RESPONSE (includes breakdown):
 * {
 *   "id": 1,
 *   "name": "John Doe",
 *   "employeeId": "EMP001",
 *   "department": "Assembly",
 *   "profilePictureUrl": "https://blob.../worker1.jpg",
 *   "violationBreakdown": {
 *     "helmetViolations": 2,
 *     "vestViolations": 1,
 *     "maskViolations": 0,
 *     "glovesViolations": 0,
 *     "totalViolations": 3
 *   },
 *   "recentViolations": [
 *     { "id": 1, "violationType": "HELMET", "detectedAt": "2024-01-15T14:32:00Z", ... },
 *     { ... }
 *   ]
 * }
 * 
 * 
 * REPORTS (HR Only)
 * ============================================================================
 * JUSTIFICATION:
 * - HR compliance and audit reporting
 * - Aggregated monthly data (not real-time)
 * - Export to Excel/PDF for regulators
 * - Identify safety trends and problem areas
 * 
 * ENDPOINTS (HR ONLY):
 * POST   /api/reports/violations-by-worker      ? Monthly violations by worker
 *        REQUEST: { year: 2024, month: 1, cameraZone?: string, violationType?: string }
 *        Response: Aggregated counts by PPE type per worker
 * 
 * POST   /api/reports/violations-by-type        ? Monthly violations by PPE type
 *        REQUEST: { year: 2024, month: 1, cameraZone?: string }
 *        Response: Total per type, top violators, top zones
 * 
 * GET    /api/reports/export/excel              ? Download Excel report
 *        QUERY: year, month, cameraZone?, violationType?
 *        Response: .xlsx file with violation data + summary
 * 
 * GET    /api/reports/export/pdf                ? Download PDF report
 *        QUERY: year, month, cameraZone?, violationType?
 *        Response: .pdf file formatted for audit submission
 * 
 * GET    /api/reports/monthly-summary           ? Dashboard metrics
 *        QUERY: year, month
 *        Response: High-level KPIs (total violations, trends, etc.)
 * 
 * RESPONSE EXAMPLE (violations-by-worker):
 * [
 *   {
 *     "workerId": 1,
 *     "workerName": "John Doe",
 *     "employeeId": "EMP001",
 *     "department": "Assembly",
 *     "totalViolations": 5,
 *     "helmetViolations": 2,
 *     "vestViolations": 1,
 *     "maskViolations": 2,
 *     "glovesViolations": 0,
 *     "periodStart": "2024-01-01T00:00:00Z",
 *     "periodEnd": "2024-01-31T23:59:59Z"
 *   }
 * ]
 * 
 * 
 * ============================================================================
 * QUERY OPTIMIZATION STRATEGIES
 * ============================================================================
 * 
 * VIOLATIONS ENDPOINT (High Traffic)
 * Problem: Factories can have thousands of violations per day
 * Solution:
 * - Pagination (always page_size=50, never return all)
 * - Database indexes on: WorkerId, CameraId, ViolationType, DetectedAt
 * - Include related entities (Worker, Camera) to avoid N+1 queries
 * - Response time requirement: <500ms for typical dashboard query
 * - Query: 
 *   SELECT v.*, w.Name, c.Zone
 *   FROM Violations v
 *   INNER JOIN Workers w ON v.WorkerId = w.Id
 *   INNER JOIN Cameras c ON v.CameraId = c.Id
 *   WHERE v.DetectedAt >= @DateFrom 
 *     AND v.DetectedAt <= @DateTo
 *     AND (v.ViolationType = @ViolationType OR @ViolationType IS NULL)
 *     AND (v.CameraId = @CameraId OR @CameraId IS NULL)
 *   ORDER BY v.DetectedAt DESC
 *   OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
 * 
 * 
 * REPORTS ENDPOINT (Heavy Aggregation)
 * Problem: GROUP BY operations with multiple aggregates
 * Solution:
 * - Cache heavily (monthly reports, rarely change after generation)
 * - Run async background job to pre-compute reports
 * - Consider separate reporting database (data warehouse)
 * - Query:
 *   SELECT 
 *     w.Id, w.Name, w.EmployeeId, w.Department,
 *     COUNT(*) as TotalViolations,
 *     SUM(CASE WHEN v.ViolationType = 'HELMET' THEN 1 ELSE 0 END) as HelmetViolations,
 *     SUM(CASE WHEN v.ViolationType = 'VEST' THEN 1 ELSE 0 END) as VestViolations,
 *     SUM(CASE WHEN v.ViolationType = 'MASK' THEN 1 ELSE 0 END) as MaskViolations,
 *     SUM(CASE WHEN v.ViolationType = 'GLOVES' THEN 1 ELSE 0 END) as GlovesViolations
 *   FROM Workers w
 *   LEFT JOIN Violations v ON w.Id = v.WorkerId
 *   WHERE v.DetectedAt >= '2024-01-01' AND v.DetectedAt < '2024-02-01'
 *   GROUP BY w.Id, w.Name, w.EmployeeId, w.Department
 *   ORDER BY TotalViolations DESC
 * 
 * 
 * CAMERAS ENDPOINT (Medium Traffic)
 * Problem: Need violation count aggregation
 * Solution:
 * - Left join to violations with count
 * - Index on Camera.IsActive, Camera.Zone
 * - Cache response (cameras rarely change)
 * - Query:
 *   SELECT c.*, COUNT(v.Id) as TotalViolations
 *   FROM Cameras c
 *   LEFT JOIN Violations v ON c.Id = v.CameraId
 *   WHERE c.IsActive = 1
 *   GROUP BY c.Id, c.CameraId, c.Zone, c.Description, c.IsActive, c.CreatedAt, c.UpdatedAt
 *   ORDER BY c.Zone
 * 
 * 
 * ============================================================================
 * PAGINATION STRATEGY
 * ============================================================================
 * All list endpoints use PagedResponse<T>:
 * {
 *   "items": [...],
 *   "totalCount": 1542,
 *   "pageNumber": 1,
 *   "pageSize": 50,
 *   "totalPages": 31,
 *   "hasNextPage": true,
 *   "hasPreviousPage": false
 * }
 * 
 * Default page size: 50 items (supervisor dashboard refresh)
 * Maximum page size: 500 items (prevent abuse)
 * 
 * Frontend implementation:
 * - First load: fetch page 1
 * - User scrolls/clicks "Next": fetch page N+1
 * - Virtual scrolling: lazy-load pages as needed
 * 
 * 
 * ============================================================================
 * ERROR HANDLING
 * ============================================================================
 * All API responses follow consistent format:
 * 
 * Success Response:
 * {
 *   "success": true,
 *   "message": "Data retrieved successfully",
 *   "data": { ... }
 * }
 * 
 * Error Response (HTTP 400 or 5xx):
 * {
 *   "success": false,
 *   "message": "Validation failed",
 *   "errors": {
 *     "field1": "Invalid format",
 *     "field2": "Required field"
 *   }
 * }
 * 
 * HTTP Status Codes:
 * - 200 OK: GET success
 * - 201 Created: POST success
 * - 204 No Content: DELETE success
 * - 400 Bad Request: Validation error
 * - 401 Unauthorized: Missing/invalid JWT token
 * - 403 Forbidden: User role insufficient
 * - 404 Not Found: Resource doesn't exist
 * - 500 Internal Server Error: Unexpected error
 * 
 * 
 * ============================================================================
 * SECURITY CONSIDERATIONS
 * ============================================================================
 * 
 * AUTHENTICATION:
 * - JWT Bearer token from /api/auth/login
 * - Token embedded with user ID, role, expiration
 * - All authenticated endpoints require: Authorization: Bearer <token>
 * - Token refresh: Use refresh_token for long-lived sessions
 * - Token expiration: 15-30 minutes (security vs. UX)
 * 
 * AUTHORIZATION:
 * - Role-based (SAFETY_SUPERVISOR, HR)
 * - [Authorize(Roles = "SAFETY_SUPERVISOR")] on supervisor endpoints
 * - [Authorize(Roles = "HR")] on HR endpoints
 * - [Authorize] on shared endpoints (enforces authentication only)
 * 
 * DATA PRIVACY:
 * - Worker profile pictures: Potentially sensitive, access control?
 * - Violation evidence images: Should be access-controlled
 * - Password: Never return in any API response
 * - LastLoginAt: Could reveal shift patterns, audit carefully
 * 
 * AUDIT TRAIL:
 * - Log all user actions (create violation, acknowledge, delete user)
 * - Include: Who, What, When, Why
 * - Store in immutable audit log table
 * - Required for compliance investigations
 * 
 * RATE LIMITING:
 * - Prevent DOS attacks on /api/violations/search (high-traffic endpoint)
 * - Suggested: 100 requests/minute per user
 * - Slower for report generation (10 requests/minute)
 * 
 * 
 * ============================================================================
 * FRONTEND PAGES & API INTEGRATION
 * ============================================================================
 * 
 * 1. LOGIN PAGE
 *    POST /api/auth/login
 *    - Store JWT token in localStorage/sessionStorage
 *    - Extract role from token claims
 *    - Redirect to appropriate dashboard based on role
 * 
 * 2. VIOLATIONS PAGE (Supervisor & HR)
 *    POST /api/violations/search (with filters)
 *    - Default filters: Today's violations, newest first
 *    - Refresh every 5-10 seconds (supervisor real-time monitoring)
 *    - Load more on scroll (pagination)
 *    - Click violation ? GET /api/violations/{id} for details
 *    - Click worker name ? GET /api/workers/{workerId} for profile
 *    - Supervisor can: PUT /api/violations/{id} to acknowledge/resolve
 * 
 * 3. CAMERAS PAGE (Supervisor Only)
 *    GET /api/cameras
 *    - Display all cameras with violation counts
 *    - Create camera ? POST /api/cameras
 *    - Edit camera ? PUT /api/cameras/{id}
 *    - Delete camera ? DELETE /api/cameras/{id}
 *    - Click camera ? GET /api/cameras/{id}/violations
 * 
 * 4. USERS PAGE (Supervisor Only)
 *    GET /api/users
 *    - Display all system users
 *    - Create user ? POST /api/users
 *    - Edit user ? PUT /api/users/{id}
 *    - Delete user ? DELETE /api/users/{id}
 *    - Reset password ? POST /api/users/{id}/reset-password
 * 
 * 5. REPORTS PAGE (HR Only)
 *    POST /api/reports/violations-by-worker (month selection)
 *    POST /api/reports/violations-by-type (month selection)
 *    GET /api/reports/export/excel (download)
 *    GET /api/reports/export/pdf (download)
 *    - Month/year picker triggers reports generation
 *    - Show loading indicator during aggregation
 *    - Cache results in localStorage for repeated access
 * 
 * 6. PROFILE PAGE (All Users)
 *    GET /api/auth/profile
 *    PUT /api/auth/profile (update name, email)
 *    PUT /api/auth/change-password
 *    - Edit name, email
 *    - Change password (requires current password)
 *    - Optional: upload profile picture (file upload endpoint needed)
 * 
 * 
 * ============================================================================
 * DATABASE INDEXES (REQUIRED FOR PERFORMANCE)
 * ============================================================================
 * 
 * Violations Table:
 * CREATE INDEX idx_violations_detectedat ON Violations(DetectedAt DESC);
 * CREATE INDEX idx_violations_workerid ON Violations(WorkerId);
 * CREATE INDEX idx_violations_cameraid ON Violations(CameraId);
 * CREATE INDEX idx_violations_type ON Violations(ViolationType);
 * CREATE INDEX idx_violations_status ON Violations(Status);
 * CREATE INDEX idx_violations_composite ON Violations(WorkerId, CameraId, DetectedAt DESC);
 * 
 * Cameras Table:
 * CREATE INDEX idx_cameras_zone ON Cameras(Zone);
 * CREATE INDEX idx_cameras_isactive ON Cameras(IsActive);
 * 
 * Workers Table:
 * CREATE INDEX idx_workers_employeeid ON Workers(EmployeeId);
 * CREATE INDEX idx_workers_department ON Workers(Department);
 * 
 * Users Table:
 * CREATE UNIQUE INDEX idx_users_username ON Users(Username);
 * CREATE UNIQUE INDEX idx_users_email ON Users(Email);
 * 
 * 
 * ============================================================================
 * FUTURE ENHANCEMENTS
 * ============================================================================
 * 
 * 1. REAL-TIME NOTIFICATIONS
 *    - WebSocket connection for live violation alerts
 *    - SignalR for supervisor dashboard real-time updates
 *    - Could push new violations without polling
 * 
 * 2. ML/AI INTEGRATION
 *    - Improve confidence score weighting
 *    - Predict high-risk violation times
 *    - Recommend focused training areas
 * 
 * 3. MOBILE APP
 *    - Supervisor app for mobile monitoring
 *    - Push notifications for critical violations
 *    - Offline-first sync strategy
 * 
 * 4. ADVANCED ANALYTICS
 *    - Trend analysis (violations over time)
 *    - Regression analysis (which factors reduce violations)
 *    - Predictive modeling (forecast violations)
 * 
 * 5. INTEGRATION WITH HR SYSTEMS
 *    - Auto-sync workers from HR database
 *    - Populate worker disciplinary records
 *    - Integration with performance review systems
 * 
 * 6. MULTI-SITE MANAGEMENT
 *    - Support for multiple factories
 *    - Site-level permissions
 *    - Cross-site reporting
 */
