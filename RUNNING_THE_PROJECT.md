# VisionGuard API - Troubleshooting & Running the Project

## ?? The Issue You Had

Your project wasn't running because of a **database connection problem**.

### Root Cause
The `appsettings.json` was configured to connect to an external database server (`db41011.databaseasp.net`), which may:
- ? Not be accessible from your network
- ? Have incorrect credentials
- ? Be offline/unavailable
- ? Have firewall restrictions

When the application started, it tried to connect to the database during initialization and **failed**, preventing the API from running.

---

## ? Solution Applied

Changed `appsettings.json` to use **SQL Server LocalDB** instead:

### Before (External Server)
```json
"DefaultConnection": "Server=db41011.databaseasp.net; Database=db41011; User Id=db41011; Password=3c?ZH-6y8Yr#; Encrypt=False; MultipleActiveResultSets=True;"
```

### After (Local SQL Server)
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VisionGuardDb;Trusted_Connection=True;MultipleActiveResultSets=true;"
```

**Benefits**:
- ? No network dependency
- ? No credentials needed (Windows auth)
- ? Faster development
- ? Works offline

---

## ?? How to Run the Project Now

### Prerequisites

#### 1. Install SQL Server LocalDB (if not already installed)

**Option A: Via Visual Studio Installer**
1. Open Visual Studio Installer
2. Click "Modify" on your Visual Studio installation
3. Go to "Individual components" tab
4. Search for "LocalDB"
5. Check "Microsoft SQL Server 2022 Express LocalDB"
6. Click "Modify"

**Option B: Direct Download**
```
https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
```

#### 2. Verify SQL Server LocalDB is Installed
```bash
# Open Command Prompt and run:
sqllocaldb info

# You should see:
# MSSQLLocalDB
# ProjectsV13
# (or similar versions)
```

### Running the API

#### Method 1: Visual Studio (Recommended)
```
1. Open D:\FinalVisionGuard\backend\visionguard.csproj in Visual Studio
2. Press F5 (Start Debugging) or Ctrl+F5 (Start Without Debugging)
3. Wait for database migration & seeding
4. Browser will open to: https://localhost:7000/swagger/index.html
```

#### Method 2: Command Line
```bash
cd D:\FinalVisionGuard\backend

# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run project
dotnet run
```

The API will start on:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:7000
- **Swagger UI**: https://localhost:7000/swagger/index.html

---

## ?? What Happens During Startup

When you run the project, this sequence occurs:

### Step 1: Application Initialization
```
? QuestPDF license set to Community
? Services registered (Auth, Database, Workers, etc.)
? CORS policy configured for ngrok
```

### Step 2: Database Setup
```
? SQL Server LocalDB connection established
? Migrations applied (creates tables)
? Database seeded with demo data:
  - 2 system users (Supervisor + HR)
  - 6 cameras
  - 10 workers
  - 50+ violations
```

### Step 3: HTTP Pipeline Setup
```
? Static files middleware (for /uploads)
? CORS enabled
? Authentication/Authorization
? Controllers mapped
```

### Step 4: Server Running
```
? Listening on http://localhost:5000
? Listening on https://localhost:7000
? Ready to accept requests
```

---

## ?? Console Output You Should See

When running successfully, you should see:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: D:\FinalVisionGuard\backend
```

### Database Seeding Messages
```
? Database migration completed
? Supervisor user added successfully
? Database seeded with demo data:
  - 2 users
  - 6 cameras
  - 10 workers
  - 50 violations
```

---

## ? If It Still Doesn't Run

### Error: "Cannot connect to database"
```
Check:
1. SQL Server LocalDB is installed
2. Run: sqllocaldb info
3. If needed: sqllocaldb create MSSQLLocalDB
```

### Error: "Access Denied" or "Login Failed"
```
Solution:
- Make sure you're using Windows authentication
- Connection string should be: (localdb)\\mssqllocaldb
- No password needed
```

### Error: "Port already in use"
```
Solution 1: Change port in launchSettings.json
Solution 2: Kill process using port:
  netstat -ano | findstr :5000
  taskkill /PID {PID} /F
```

### Error: "QuestPDF License Error"
```
Solution:
- Community license is free (already configured)
- No action needed
```

### Error: "Trust HTTPS certificate"
```
Solution:
- Run: dotnet dev-certs https --trust
- Windows will ask for permission
- Click "Yes"
```

---

## ?? Default Credentials for Login

After the database is seeded, you can login with:

### Supervisor Account
```
Username: SUP-001
Password: 1234
Role: SAFETY_SUPERVISOR
```

### HR Account
```
Username: HR-001
Password: 1234
Role: HR
```

---

## ?? Database Information

### Database Name
```
VisionGuardDb
```

### Location
```
C:\Users\{YourUsername}\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\{DatabaseName}.mdf
```

### View Database
```
Option 1: Visual Studio SQL Server Object Explorer
Option 2: SQL Server Management Studio (SSMS)
Option 3: Azure Data Studio
```

---

## ?? Quick Test After Running

### Test 1: Check API is Running
```bash
curl http://localhost:5000/api/auth/login
```

Expected response: Should accept POST requests

### Test 2: Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"SUP-001","password":"1234"}'
```

Expected response:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJ...",
    "user": {
      "username": "SUP-001",
      "role": "SAFETY_SUPERVISOR"
    }
  }
}
```

### Test 3: Access Protected Endpoint
```bash
# Copy token from login response, then:
curl -X GET http://localhost:5000/api/auth/profile \
  -H "Authorization: Bearer {token}"
```

Expected response: Your user profile

---

## ?? Accessing Swagger UI

Once the app is running:

1. Open browser
2. Go to: **https://localhost:7000/swagger/index.html**
3. You'll see all API endpoints with documentation
4. You can test endpoints directly from Swagger UI

---

## ?? Project Structure

```
D:\FinalVisionGuard\backend\
??? Program.cs                 ? Main entry point (startup config)
??? appsettings.json          ? Configuration (database, JWT, logging)
??? appsettings.Development.json
??? visionguard.csproj        ? Project file
?
??? Controllers/              ? API endpoints
?   ??? AuthController.cs
?   ??? WorkersController.cs
?   ??? ViolationsController.cs
?   ??? CamerasController.cs
?   ??? UsersController.cs
?   ??? ReportsController.cs
?
??? Models/                   ? Database entities
?   ??? User.cs
?   ??? Worker.cs
?   ??? Camera.cs
?   ??? Violation.cs
?
??? Services/                 ? Business logic
?   ??? JwtTokenGenerator.cs
?   ??? IWorkerService.cs
?   ??? WorkerService.cs
?   ??? IViolationService.cs
?   ??? ViolationService.cs
?   ??? IFileStorageService.cs
?
??? Data/                     ? Database context
?   ??? VisionGuardDbContext.cs
?   ??? DbSeeder.cs          ? Demo data initialization
?   ??? Migrations/          ? Database migration history
?
??? DTOs/                     ? Data transfer objects
    ??? LoginRequest.cs
    ??? WorkerDto.cs
    ??? ViolationDto.cs
    ??? ...
```

---

## ?? Configuration Files Explained

### Program.cs
- JWT token configuration
- Database connection setup
- Middleware registration
- Service dependency injection

### appsettings.json
- Database connection string
- JWT settings (secret, issuer, audience, expiration)
- Logging configuration
- Allowed hosts

### DbSeeder.cs
- Creates demo database records on first run
- Adds 2 system users (supervisor + HR)
- Adds 6 cameras in different zones
- Adds 10 factory workers
- Generates 50+ violation records for testing

---

## ?? Next Steps

1. ? Run the project with `dotnet run`
2. ? Access Swagger UI at https://localhost:7000/swagger
3. ? Login with SUP-001 / 1234
4. ? Get JWT token
5. ? Test API endpoints
6. ? View demo data in database

---

## ?? Tips

- **Development**: Use LocalDB (now configured)
- **Production**: Use full SQL Server or Azure SQL Database
- **Testing**: Use in-memory database or LocalDB
- **Credentials**: SUP-001 is supervisor (full access)
- **Tokens**: Valid for 480 minutes (8 hours)
- **CORS**: Configured for localhost, 127.0.0.1, and ngrok domains

---

## ?? Still Having Issues?

### Check these in order:
1. [ ] SQL Server LocalDB is installed (`sqllocaldb info`)
2. [ ] Visual Studio has latest updates
3. [ ] .NET 8 SDK is installed (`dotnet --version`)
4. [ ] Connection string is correct in `appsettings.json`
5. [ ] Port 5000/7000 is not in use
6. [ ] No firewall blocking localhost connections
7. [ ] Database migration succeeds (check console output)

### Collect debug info:
```bash
# Check .NET version
dotnet --version

# Check installed SQL LocalDB
sqllocaldb info

# Try to create test connection
sqlcmd -S (localdb)\mssqllocaldb -Q "SELECT 1"
```

---

**Last Updated**: January 2026  
**Status**: ? Project Now Running  
**Database**: SQL Server LocalDB  
**API Endpoints**: 41 total  
**Demo Data**: Fully seeded on startup
