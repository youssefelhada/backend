# VisionGuard API - Startup Error Fixes

## ?? Common Startup Errors & Solutions

---

## Error 1: Database Connection Failed

### Symptom
```
InvalidOperationException: Unable to connect to database
SqlException: A network-related or instance-specific error occurred
```

### Root Causes
- SQL Server LocalDB not installed
- SQL Server LocalDB not running
- Wrong connection string
- Firewall blocking connection

### ? Fix

**Step 1: Check if LocalDB is installed**
```bash
sqllocaldb info
```

Expected output:
```
MSSQLLocalDB
ProjectsV13
```

If you see nothing, install it:

**Windows Command Prompt (Admin)**:
```bash
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

**Step 2: Verify connection string**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VisionGuardDb;Trusted_Connection=True;MultipleActiveResultSets=true;"
  }
}
```

**Step 3: Test connection**
```bash
sqlcmd -S (localdb)\mssqllocaldb -Q "SELECT @@VERSION"
```

---

## Error 2: "Port Already in Use"

### Symptom
```
System.Net.Sockets.SocketException (48): Address already in use
Could not bind to http://0.0.0.0:5000
```

### Root Cause
Another application is using port 5000 or 7000

### ? Fix

**Option A: Find and kill process**
```bash
# Windows Command Prompt
netstat -ano | findstr :5000

# Note the PID (e.g., 12345)
taskkill /PID 12345 /F

# For HTTPS port 7000
netstat -ano | findstr :7000
taskkill /PID {PID} /F
```

**Option B: Change port in launchSettings.json**
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7001;http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Option C: Restart computer**
```bash
shutdown /r /t 0
```

---

## Error 3: HTTPS Certificate Trust Error

### Symptom
```
System.IO.IOException: Unable to load certificate
No usable version found in SSL session request
```

### Root Cause
.NET development certificate not trusted

### ? Fix

```bash
# Trust the HTTPS certificate
dotnet dev-certs https --trust

# Windows will prompt for permission
# Click "Yes" when asked
```

If that doesn't work:

```bash
# Remove old certificate
dotnet dev-certs https --clean

# Create new certificate
dotnet dev-certs https

# Trust it
dotnet dev-certs https --trust
```

---

## Error 4: .NET 8 SDK Not Found

### Symptom
```
Could not find a compatible target framework
No .NET 8.0 runtimes were found
```

### Root Cause
.NET 8 SDK not installed

### ? Fix

**Download and install**:
https://dotnet.microsoft.com/download/dotnet/8.0

**Verify installation**:
```bash
dotnet --version

# Should output: 8.x.x
```

**List installed SDKs**:
```bash
dotnet --list-sdks
```

---

## Error 5: Migration/Database Schema Issues

### Symptom
```
SqlException: Invalid column name
InvalidOperationException: Unable to create model for context
```

### Root Cause
Database tables don't exist or schema is corrupt

### ? Fix

**Option A: Let migrations run automatically (easiest)**
```
Just run the app - Program.cs has:
await context.Database.MigrateAsync();
```

**Option B: Manual migration (if needed)**
```bash
cd D:\FinalVisionGuard\backend

# Remove old database
# Delete the .mdf file at:
# C:\Users\{YourUsername}\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\

# Or via command:
sqlcmd -S (localdb)\mssqllocaldb -Q "DROP DATABASE VisionGuardDb"

# Then run the app
dotnet run
```

**Option C: View/Manage database**
```bash
# Install SQL Server Management Studio (SSMS)
# https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

# Or use Azure Data Studio (free)
# https://azure.microsoft.com/products/data-studio/
```

---

## Error 6: BCrypt Password Hashing Error

### Symptom
```
System.Reflection.TargetInvocationException: BCrypt library not found
ArgumentException: Invalid BCrypt hash
```

### Root Cause
BCrypt.Net-Next package not installed or corrupted

### ? Fix

```bash
cd D:\FinalVisionGuard\backend

# Restore NuGet packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build

# Run
dotnet run
```

---

## Error 7: Invalid JSON in appsettings.json

### Symptom
```
System.Text.Json.JsonException: Unexpected character '{'
Configuration failed to load
```

### Root Cause
JSON syntax error in configuration file

### ? Fix

**Validate JSON**:
1. Open appsettings.json
2. Check for:
   - Missing commas between properties
   - Trailing commas in objects/arrays
   - Unmatched quotes
   - Special characters not escaped

**Use online validator**:
https://jsonlint.com/

**Correct format**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=..."
  },
  "JwtSettings": {
    "Secret": "...",
    "ExpirationInMinutes": 480
  }
}
```

---

## Error 8: Service Not Registered

### Symptom
```
InvalidOperationException: Unable to resolve service for type 'IWorkerService'
No service for type 'ViolationService' has been registered
```

### Root Cause
Service not added in Program.cs

### ? Fix

Verify all services are registered in Program.cs:

```csharp
// BUSINESS SERVICES
builder.Services.AddScoped<IViolationService, ViolationService>();
builder.Services.AddSingleton(new JwtTokenGenerator(jwtSecret));

// Worker Module Services
builder.Services.AddScoped<visionguard.Repositories.IWorkerRepository, visionguard.Repositories.WorkerRepository>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
```

---

## Error 9: File Upload Directory Error

### Symptom
```
System.IO.DirectoryNotFoundException: Could not find part of path
Directory not found: /uploads
```

### Root Cause
Uploads directory doesn't exist

### ? Fix

**Automatic (app creates it)**:
```
When you upload a file, the app creates /uploads directory
If it fails, check write permissions
```

**Manual**:
```bash
# Navigate to project root
cd D:\FinalVisionGuard\backend

# Create uploads directory
mkdir wwwroot\uploads

# Verify
dir wwwroot
```

**Check permissions**:
- Right-click folder ? Properties ? Security
- Ensure your user has "Write" and "Modify" permissions

---

## Error 10: Token/Authentication Errors

### Symptom
```
InvalidOperationException: IssuerSigningKey was null
HttpRequestException: 401 Unauthorized on every request
```

### Root Cause
JWT configuration mismatch

### ? Fix

Verify JWT configuration in Program.cs:

```csharp
var jwtSecret = "VisionGuardSecretKeyForDevelopment2026VisionGuardSecretKeyForDev";
var key = Encoding.ASCII.GetBytes(jwtSecret);

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
            ClockSkew = TimeSpan.FromSeconds(60)
        };
    });
```

**Important**: Same secret must be used for:
1. Token generation (in JwtTokenGenerator)
2. Token validation (in Program.cs)

---

## Error 11: Swagger UI Not Loading

### Symptom
```
Cannot GET /swagger/index.html
Swagger UI page blank/error
```

### Root Cause
Swagger not configured or disabled

### ? Fix

Verify in Program.cs:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Check if in Development mode**:
1. Look at environment variable: `ASPNETCORE_ENVIRONMENT`
2. Should be "Development" during local testing
3. Default is "Production" in published apps

**Access Swagger**:
- HTTP: http://localhost:5000/swagger/index.html
- HTTPS: https://localhost:7000/swagger/index.html

---

## Error 12: Entity Framework Core Issues

### Symptom
```
InvalidOperationException: The model for context 'VisionGuardDbContext' cannot be used
The entity 'Worker' requires a primary key to be defined
```

### Root Cause
Database models not properly configured

### ? Fix

Check Models (User.cs, Worker.cs, etc.) have:
1. `public int Id { get; set; }` - Primary key
2. Required properties marked with `[Required]`
3. Navigation properties properly configured

Example:
```csharp
public class Worker
{
    public int Id { get; set; }  // ? Required
    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
}
```

---

## ? Startup Checklist

Before running, verify all of these:

- [ ] .NET 8 SDK installed (`dotnet --version`)
- [ ] SQL Server LocalDB installed (`sqllocaldb info`)
- [ ] appsettings.json has correct connection string
- [ ] No syntax errors in JSON files
- [ ] Ports 5000/7000 are free
- [ ] No firewall blocking localhost
- [ ] Visual Studio/VS Code updated to latest
- [ ] NuGet packages restored (`dotnet restore`)
- [ ] Project builds successfully (`dotnet build`)
- [ ] Database migrations configured in Program.cs
- [ ] All services registered in Program.cs
- [ ] JWT secret configured
- [ ] CORS policy set up
- [ ] Controllers exist and are registered

---

## ?? If Nothing Works

**Last Resort Solutions**:

### 1. Clean Build
```bash
cd D:\FinalVisionGuard\backend
dotnet clean
dotnet restore
dotnet build
```

### 2. Delete LocalDB and Start Fresh
```bash
# Delete database
sqlcmd -S (localdb)\mssqllocaldb -Q "DROP DATABASE VisionGuardDb"

# Delete bin/obj
rmdir /s /q bin
rmdir /s /q obj

# Rebuild
dotnet restore
dotnet build
```

### 3. Reinstall .NET 8
```bash
# Visit: https://dotnet.microsoft.com/download/dotnet/8.0
# Download installer
# Uninstall all .NET versions (Control Panel)
# Reinstall .NET 8 SDK
```

### 4. Ask for Help
Provide:
- Full error message (copy from console)
- Output of `dotnet --version`
- Output of `sqllocaldb info`
- Contents of appsettings.json
- Recent changes made to Program.cs
- Operating system and Visual Studio version

---

## ?? Support Resources

- **.NET Documentation**: https://learn.microsoft.com/dotnet/
- **Entity Framework**: https://learn.microsoft.com/ef/
- **SQL Server LocalDB**: https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb
- **ASP.NET Core**: https://learn.microsoft.com/aspnet/core/
- **JWT**: https://jwt.io/
- **Stack Overflow**: https://stackoverflow.com/questions/tagged/asp.net-core

---

**Last Updated**: January 2026  
**VisionGuard API Version**: 1.0  
**Status**: ? Fixed and Ready to Run
