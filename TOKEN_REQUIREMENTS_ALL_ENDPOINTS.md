# VisionGuard API - Token Requirements for Every Endpoint

## ?? Authentication Token Guide

This document outlines the JWT token requirements, generation, and usage for all 41 API endpoints.

---

## ?? Token Overview

**Token Type**: JWT (JSON Web Token)  
**Algorithm**: HS256 (HMAC SHA-256)  
**Default Expiration**: 480 minutes (8 hours)  
**Clock Skew**: 60 seconds (for ngrok compatibility)  
**Validation**: Signature + Lifetime

### Token Structure
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
```

**Three Parts**:
1. Header: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9` (algorithm info)
2. Payload: `eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ` (claims)
3. Signature: `SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c` (verification)

---

## ?? Token Generation

### How to Get a Token

**Endpoint**: `POST /api/auth/login`  
**Authentication**: ? Not Required

**Request**:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "supervisor",
    "password": "password123"
  }'
```

**Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
    "user": {
      "id": 1,
      "username": "supervisor",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "department": "Safety & Compliance",
      "role": "SAFETY_SUPERVISOR",
      "profilePicturePath": null
    }
  }
}
```

### Token Claims (Payload)
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "supervisor",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "SAFETY_SUPERVISOR",
  "exp": 1705262400,
  "iss": "VisionGuardApi",
  "aud": "VisionGuardClient"
}
```

**Claims Explanation**:
- `nameidentifier`: User ID (used to identify the user)
- `name`: Username
- `role`: User role (SAFETY_SUPERVISOR or HR)
- `exp`: Expiration timestamp (Unix time)
- `iss`: Issuer (VisionGuardApi)
- `aud`: Audience (VisionGuardClient)

---

## ?? Using the Token

### Authorization Header Format
```
Authorization: Bearer {token}
```

### Example Request with Token
```bash
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
```

### Postman Example
```
Headers:
- Key: Authorization
- Value: Bearer {paste_token_here}
- Type: Text
```

---

## ?? Token Requirements by Endpoint

### **AUTHENTICATION (7 endpoints)**

#### 1. POST /api/auth/login
```
Token Required: ? NO
Role Required: NONE
Policy Required: NONE
```
**Description**: Login with username/password to get token  
**Usage**: First endpoint to call to get a token

---

#### 2. GET /api/auth/profile
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```
**Example**:
```bash
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer {token}"
```

---

#### 3. PUT /api/auth/profile
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

---

#### 4. PUT /api/auth/change-password
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

---

#### 5. POST /api/auth/upload-avatar
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

---

#### 6. POST /api/auth/refresh
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
Status: ? TODO (Not yet implemented)
```
**Headers**:
```
Authorization: Bearer {token}
```

---

#### 7. POST /api/auth/logout
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

### **WORKERS (5 endpoints)**

#### 1. GET /api/workers
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

#### 2. GET /api/workers/{id}
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

#### 3. POST /api/workers
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

---

#### 4. PUT /api/workers/{id}
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

---

#### 5. DELETE /api/workers/{id}
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

### **VIOLATIONS (5 endpoints)**

#### 1. POST /api/violations
```
Token Required: ? NO (?? AI Detection Endpoint)
Role Required: NONE
Policy Required: NONE
```
**Description**: Public endpoint for AI model to record violations  
**?? Warning**: Should be protected with API key in production

---

#### 2. POST /api/violations/search
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

---

#### 3. GET /api/violations/{id}
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

#### 4. PUT /api/violations/{id}
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: SupervisorOnly
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Note**: Only supervisors can update violations

---

#### 5. GET /api/violations/statistics/dashboard
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

### **CAMERAS (6 endpoints)**

#### 1. GET /api/cameras
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

#### 2. GET /api/cameras/{id}
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```

---

#### 3. POST /api/cameras
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Note**: Only supervisors can create cameras

---

#### 4. PUT /api/cameras/{id}
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Note**: Only supervisors can update cameras

---

#### 5. DELETE /api/cameras/{id}
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
```
**Note**: Only supervisors can delete cameras (soft-delete)

---

#### 6. GET /api/cameras/{id}/violations
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```
**Query Parameters**:
```
?pageNumber=1&pageSize=50
```

---

### **USERS (6 endpoints)**

#### 1. GET /api/users
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
```
**Query Parameters**:
```
?pageNumber=1&pageSize=100&roleFilter=SAFETY_SUPERVISOR (optional)
```
**Note**: Only supervisors can view all users

---

#### 2. GET /api/users/{id}
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
```
**Note**: Only supervisors can view specific users

---

#### 3. POST /api/users
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Note**: Only supervisors can create users

---

#### 4. PUT /api/users/{id}
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Note**: Only supervisors can update users

---

#### 5. DELETE /api/users/{id}
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
```
**Restrictions**:
- Cannot delete SAFETY_SUPERVISOR users
- Cannot delete HR-001 (main HR user)

---

#### 6. POST /api/users/{id}/reset-password
```
Token Required: ? YES
Role Required: SAFETY_SUPERVISOR
Policy Required: NONE (Role-based: Roles = "SAFETY_SUPERVISOR")
```
**Headers**:
```
Authorization: Bearer {token}
```
**Note**: Only supervisors can reset passwords

---

### **REPORTS (5 endpoints)**

#### 1. POST /api/reports/violations-by-worker
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

---

#### 2. POST /api/reports/violations-by-type
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

---

#### 3. GET /api/reports/monthly-summary
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
```
**Query Parameters**:
```
?year=2026&month=1
```

---

#### 4. POST /api/reports/export-excel
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Response**: Binary file (Excel)

---

#### 5. POST /api/reports/export-pdf
```
Token Required: ? YES
Role Required: ANY
Policy Required: NONE
```
**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```
**Response**: Binary file (PDF)

---

## ?? Token Statistics Summary

| Category | Total | Token Required | Public |
|----------|-------|-----------------|--------|
| Authentication | 7 | 6 | 1 |
| Workers | 5 | 5 | 0 |
| Violations | 5 | 4 | 1 |
| Cameras | 6 | 6 | 0 |
| Users | 6 | 6 | 0 |
| Reports | 5 | 5 | 0 |
| **TOTAL** | **41** | **38** | **2** |

---

## ??? Authorization Levels

### Level 1: No Authentication Required (2 endpoints)
```
? /api/auth/login
? /api/violations (AI Detection)
```

### Level 2: Any Authenticated User (22 endpoints)
Requires valid JWT token but no specific role.
```
? /api/auth/profile (GET, PUT)
? /api/auth/change-password
? /api/auth/upload-avatar
? /api/auth/logout
? /api/workers (GET all, GET by ID, POST, PUT, DELETE)
? /api/violations/search
? /api/violations/{id} (GET)
? /api/violations/statistics/dashboard
? /api/cameras (GET all, GET by ID)
? /api/cameras/{id}/violations
? /api/reports/* (all 5 report endpoints)
```

### Level 3: SAFETY_SUPERVISOR Role Required (13 endpoints)
Requires JWT token + SAFETY_SUPERVISOR role.
```
? /api/violations/{id} (PUT) - Policy: SupervisorOnly
? /api/cameras (POST, PUT, DELETE)
? /api/users (GET all, GET by ID, POST, PUT, DELETE)
? /api/users/{id}/reset-password
```

### Level 4: HR Role Required (0 endpoints)
Currently no endpoints require HR role only.  
HR users can access all authenticated endpoints but cannot perform supervisor actions.

---

## ?? Token Refresh Flow

```
1. User logs in ? GET accessToken (expires in 8 hours)
   
2. Use token in Authorization header for all requests
   
3. When token expires ? GET 401 Unauthorized
   
4. POST /api/auth/refresh (? TODO)
   ? GET new accessToken
   
5. Use new token for subsequent requests
```

**Current Status**: Token refresh is not yet implemented.  
**Workaround**: User must login again when token expires.

---

## ?? Common Token Issues

### Issue 1: 401 Unauthorized on Valid Token
**Possible Causes**:
- Token expired (8 hour limit)
- Token in wrong format (missing "Bearer " prefix)
- Token corrupted or invalid signature
- Clock skew too large (server time differs from client)

**Solution**:
```bash
# Verify token format
# Should be: Authorization: Bearer {token}

# Decode token at jwt.io to check:
# - Expiration time (exp claim)
# - Issued time (iat claim)
# - Role (role claim)
```

---

### Issue 2: 403 Forbidden (Insufficient Permission)
**Possible Causes**:
- User doesn't have required role
- Token is for non-supervisor user
- Policy not satisfied

**Example**:
```
Trying: POST /api/cameras (requires SAFETY_SUPERVISOR)
With: HR user token
Result: 403 Forbidden
```

**Solution**:
- Use token from supervisor account
- Or: /api/users endpoint (only available to supervisors)

---

### Issue 3: Token Not Being Passed Correctly
**Incorrect**:
```
Authorization: {token}
Authorization: Token {token}
Authorization: JWT {token}
```

**Correct**:
```
Authorization: Bearer {token}
```

---

## ?? Token Configuration (Program.cs)

### Current Settings
```csharp
var jwtSecret = "VisionGuardSecretKeyForDevelopment2026VisionGuardSecretKeyForDev";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(60)  // For ngrok
        };
        options.SaveToken = true;
        options.IncludeErrorDetails = true;
    });
```

### Token Generation Settings (JwtTokenGenerator.cs)
```csharp
public string GenerateToken(int userId, string username, string role, int expirationMinutes = 480)
{
    // 480 minutes = 8 hours
    // Claims: userId, username, role
}
```

---

## ?? Testing Token Validity

### Method 1: Decode at jwt.io
1. Go to https://jwt.io
2. Paste your token in the "Encoded" section
3. View the decoded payload
4. Verify expiration time hasn't passed

### Method 2: Test with cURL
```bash
# Test with token
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer {token}" \
  -v

# Check response headers for auth details
# Look for 200 OK (success) or 401 Unauthorized (failure)
```

### Method 3: Postman Collection
1. Login to get token
2. Copy token from response
3. Set in Postman environment variable: `{{token}}`
4. Use in Authorization header
5. Subsequent requests automatically include it

---

## ?? Token Lifecycle Example

```
T=0:00   User logs in
         POST /api/auth/login
         ? Returns: accessToken (expires at T=8:00)

T=0:05   User clicks "View Profile"
         GET /api/auth/profile
         Headers: Authorization: Bearer {token}
         ? Success (200 OK)

T=0:30   User updates profile
         PUT /api/auth/profile
         Headers: Authorization: Bearer {token}
         ? Success (200 OK)

T=8:00   Token expires automatically
         (No action needed from user yet)

T=8:05   User tries to get violations
         GET /api/violations
         Headers: Authorization: Bearer {expired_token}
         ? Failure (401 Unauthorized)

T=8:05   User must login again
         POST /api/auth/login
         ? Returns: NEW accessToken (expires at T=16:05)

T=8:10   User can now access endpoints again
         GET /api/violations
         Headers: Authorization: Bearer {new_token}
         ? Success (200 OK)
```

---

## ?? Quick Start: Getting Your First Token

### Step 1: Ensure API is Running
```bash
cd D:\FinalVisionGuard\backend
dotnet run
# Or press F5 in Visual Studio
```

### Step 2: Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"supervisor","password":"password123"}'
```

### Step 3: Copy Token
From response, copy the `accessToken` value

### Step 4: Test with Token
```bash
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer {paste_token_here}"
```

### Step 5: Should See Your Profile
```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "supervisor",
    "firstName": "John",
    "role": "SAFETY_SUPERVISOR"
  }
}
```

---

## ?? Related Files

- `Program.cs` - JWT configuration
- `Services/JwtTokenGenerator.cs` - Token generation logic
- `Controllers/AuthController.cs` - Login endpoint
- `appsettings.json` - JWT settings (secret key)

---

## ? Token Requirements Checklist

When testing any endpoint:
- [ ] Check if token is required (? or ?)
- [ ] Check required role (if any)
- [ ] Check required policy (if any)
- [ ] Include in Authorization header: `Bearer {token}`
- [ ] Verify token hasn't expired
- [ ] Verify user has required role
- [ ] Include correct Content-Type header

---

**Last Updated**: January 2026  
**API Version**: 1.0  
**Total Endpoints Documented**: 41  
**Token Type**: JWT (HS256)  
**Default Expiration**: 480 minutes (8 hours)
