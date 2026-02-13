# SAFETY_ENGINEER Role Implementation Guide

## ?? Overview

Added a new **SAFETY_ENGINEER** role to the system to support mobile app admin functionality. This allows the mobile app to create users with the `SAFETY_ENGINEER` role **without affecting existing React app roles** (SAFETY_SUPERVISOR and HR).

---

## ?? What Changed

### 1. **UserRole Enum** (Models/User.cs)
```csharp
public enum UserRole
{
    SAFETY_SUPERVISOR,  // Existing - React Admin
    HR,                 // Existing - React HR Staff
    SAFETY_ENGINEER     // NEW - Mobile App Field Engineer
}
```

**Impact**: ? Backward compatible - existing roles unchanged

---

### 2. **New Demo User** (Data/DbSeeder.cs)
```csharp
new User
{
    Username = "ENG-001",
    Email = "engineer@visionguard.local",
    PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
    FirstName = "Safety",
    LastName = "Engineer",
    EmployeeId = "ENG-001",
    Department = "Safety Engineering",
    Role = UserRole.SAFETY_ENGINEER,
    IsActive = true,
    CreatedAt = DateTime.UtcNow
}
```

**Demo Login**:
- Username: `ENG-001`
- Password: `1234`

---

### 3. **Authorization Policy** (Program.cs)
```csharp
.AddPolicy("EngineerOnly", policy => 
    policy.RequireRole("SAFETY_ENGINEER"))
```

**Usage**: Apply to endpoints that require engineer access

---

### 4. **Updated DTOs** (DTOs/UserDto.cs)
- UserDto: Role can now be "SAFETY_SUPERVISOR", "HR", or "SAFETY_ENGINEER"
- CreateUserRequest: Mobile app can pass "SAFETY_ENGINEER" as role
- UpdateUserRequest: Admin can change user role to/from "SAFETY_ENGINEER"

---

## ?? How Mobile App Admin Can Create SAFETY_ENGINEER

### Endpoint: `POST /api/users`

**Request**:
```json
{
  "username": "field-eng-001",
  "email": "fieldeng@visionguard.local",
  "firstName": "John",
  "lastName": "Field",
  "employeeId": "FE-001",
  "department": "Field Engineering",
  "role": "SAFETY_ENGINEER",
  "password": "SecurePassword123!"
}
```

**Response**:
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "id": 4,
    "username": "field-eng-001",
    "firstName": "John",
    "lastName": "Field",
    "email": "fieldeng@visionguard.local",
    "employeeId": "FE-001",
    "department": "Field Engineering",
    "role": "SAFETY_ENGINEER",
    "isActive": true,
    "createdAt": "2026-01-15T10:00:00Z"
  }
}
```

**Requirements**:
- ? Mobile app admin must have SAFETY_SUPERVISOR role
- ? Valid JWT token in Authorization header
- ? POST request to `/api/users`

---

## ?? Role-Based Access Control

### Current Role Hierarchy

| Role | Can Create Users | Can Manage Cameras | Can View Violations | Can Export Reports |
|------|------------------|-------------------|---------------------|-------------------|
| SAFETY_SUPERVISOR | ? Yes | ? Yes | ? Yes | ? Yes |
| HR | ? No | ? No | ? Yes | ? Yes |
| SAFETY_ENGINEER | ? No | ? No | ? Yes | ? Yes |

**Note**: To add specific permissions for SAFETY_ENGINEER, modify individual controller endpoints.

---

## ?? Example: Restrict Endpoint to SAFETY_ENGINEER Only

To create an engineer-only endpoint, use the new policy:

```csharp
[HttpGet("engineer-report")]
[Authorize(Policy = "EngineerOnly")]
public async Task<IActionResult> GetEngineerReport()
{
    // Only SAFETY_ENGINEER can access
    return Ok(new { data = "Engineer-specific data" });
}
```

---

## ?? React App Impact

? **No Impact on React App**

The React app continues to work with:
- `SAFETY_SUPERVISOR` - Full admin access
- `HR` - Reports and violations only

The new `SAFETY_ENGINEER` role is used exclusively by the mobile app and does not appear in React components (unless explicitly added).

---

## ?? Mobile App Usage

### Mobile Admin Can:
1. ? Create users with `SAFETY_ENGINEER` role
2. ? Create users with `SAFETY_SUPERVISOR` role
3. ? Create users with `HR` role
4. ? Update user roles
5. ? Reset user passwords
6. ? Delete users (with restrictions)

### Mobile Admin Cannot:
- ? Change their own role
- ? Delete SAFETY_SUPERVISOR users (protected)
- ? Delete HR-001 (protected)

---

## ?? Testing

### 1. Login as SAFETY_ENGINEER
```bash
curl -X POST https://localhost:7185/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"ENG-001","password":"1234"}'
```

**Response**:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "user": {
      "username": "ENG-001",
      "role": "SAFETY_ENGINEER"
    }
  }
}
```

### 2. Create New SAFETY_ENGINEER (as SAFETY_SUPERVISOR)
```bash
curl -X POST https://localhost:7185/api/users \
  -H "Authorization: Bearer {supervisor_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "eng-002",
    "email": "eng002@visionguard.local",
    "firstName": "Jane",
    "lastName": "Engineer",
    "employeeId": "ENG-002",
    "department": "Safety Engineering",
    "role": "SAFETY_ENGINEER",
    "password": "SecurePass123!"
  }'
```

### 3. View All Users (Supervisor Can See All Roles)
```bash
curl -X GET https://localhost:7185/api/users \
  -H "Authorization: Bearer {supervisor_token}"
```

**Response** includes users with all three roles:
- SAFETY_SUPERVISOR
- HR
- SAFETY_ENGINEER

---

## ?? Database Impact

### Users Table Now Supports:
- All existing SAFETY_SUPERVISOR users: ? Unchanged
- All existing HR users: ? Unchanged
- New SAFETY_ENGINEER users: ? Fully supported

### Migration Status:
- ? No migration needed (enum added)
- ? Database auto-created with new values
- ? Backward compatible

---

## ??? Security Considerations

### Protected Users (Cannot Be Deleted):
1. ? All SAFETY_SUPERVISOR users (role protection)
2. ? HR-001 (hardcoded protection)

### New Considerations for SAFETY_ENGINEER:
- Decide if SAFETY_ENGINEER users can be deleted
- Decide if certain operations are restricted to SAFETY_ENGINEER
- Consider audit logging for engineer actions

---

## ?? API Endpoints - User Management

All existing endpoints work with the new role:

| Endpoint | Method | SAFETY_SUPERVISOR | HR | SAFETY_ENGINEER |
|----------|--------|-------------------|-----|-----------------|
| /api/users | GET | ? | ? | ? |
| /api/users/{id} | GET | ? | ? | ? |
| /api/users | POST | ? | ? | ? |
| /api/users/{id} | PUT | ? | ? | ? |
| /api/users/{id} | DELETE | ?* | ? | ? |
| /api/users/{id}/reset-password | POST | ? | ? | ? |

*Cannot delete SAFETY_SUPERVISOR or HR-001

---

## ?? Checklist - Implementation Complete

- ? Added `SAFETY_ENGINEER` to UserRole enum
- ? Created demo user (ENG-001) in DbSeeder
- ? Added "EngineerOnly" authorization policy
- ? Updated UserDto comments
- ? Updated CreateUserRequest comments
- ? Updated UpdateUserRequest comments
- ? Database schema backward compatible
- ? React app unaffected
- ? Build successful

---

## ?? Next Steps

### Optional Enhancements:

1. **Create Engineer-Only Endpoints**
   ```csharp
   [Authorize(Policy = "EngineerOnly")]
   public IActionResult GetEngineerDashboard()
   {
       // Engineer-specific dashboard
   }
   ```

2. **Add Field-Level Permissions**
   ```csharp
   // Example: Engineers can only view violations from specific cameras
   if (user.Role == UserRole.SAFETY_ENGINEER)
   {
       violations = violations.Where(v => 
           v.Camera.Zone == user.AssignedZone);
   }
   ```

3. **Add Audit Logging**
   ```csharp
   // Log when SAFETY_ENGINEER creates/updates records
   _auditLog.LogAction(user.Id, "CREATE_VIOLATION", violation.Id);
   ```

4. **Mobile App UI**
   - Show role selection dropdown when creating users
   - Highlight SAFETY_ENGINEER in user list
   - Restrict certain operations by role

---

## ?? Related Files

- `Models/User.cs` - UserRole enum
- `Data/DbSeeder.cs` - Demo user creation
- `DTOs/UserDto.cs` - DTO documentation
- `Program.cs` - Authorization policies
- `Controllers/UsersController.cs` - User management endpoints

---

## ? Backward Compatibility

- ? React app continues to work without changes
- ? Existing SAFETY_SUPERVISOR tokens still valid
- ? Existing HR tokens still valid
- ? No database migrations required
- ? All existing endpoints functional

---

**Status**: ? Complete and Ready to Use  
**Impact**: ?? No breaking changes to React app  
**Mobile App**: ?? Ready to create SAFETY_ENGINEER users  
**Build**: ? Successful
