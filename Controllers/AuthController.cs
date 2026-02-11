using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using visionguard.Data;
using visionguard.DTOs;
using visionguard.Services;

namespace visionguard.Controllers
{
    /// <summary>
    /// Authentication Controller
    /// Handles user login and token generation
    /// 
    /// JUSTIFICATION:
    /// - Frontend login page needs JWT token for subsequent requests
    /// - Token embedded with role (SAFETY_SUPERVISOR or HR)
    /// - Token used for authorization on all other endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly VisionGuardDbContext _context;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly IWebHostEnvironment _environment;

        public AuthController(VisionGuardDbContext context, JwtTokenGenerator tokenGenerator, IWebHostEnvironment environment)
        {
            _context = context;
            _tokenGenerator = tokenGenerator;
            _environment = environment;
        }
        /// <summary>
        /// POST /api/auth/login
        /// Authenticates user and returns JWT token
        /// 
        /// BUSINESS LOGIC:
        /// 1. Validate credentials against User table
        /// 2. Generate JWT token with user role embedded
        /// 3. Return token + user profile for dashboard initialization
        /// 
        /// FRONTEND USAGE:
        /// - Login page submits username/password
        /// - Receives token, stores in localStorage/sessionStorage
        /// - Frontend includes token in Authorization header for all requests
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Username and password are required"
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            // Generate JWT token
            var token = _tokenGenerator.GenerateToken(user.Id, user.Username, user.Role.ToString());

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var response = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = token, // In production, use separate refresh token
                User = new UserProfileDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Department = user.Department,
                    Role = user.Role.ToString(),
                    ProfilePicturePath = user.ProfilePicturePath
                }
            };

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            });
        }

        /// <summary>
        /// GET /api/auth/profile
        /// Returns current authenticated user's profile
        /// 
        /// JUSTIFICATION:
        /// - Profile page displays user info
        /// - Dashboard header shows user name and role
        /// - Used to initialize app state on page load
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department,
                Role = user.Role.ToString(),
                ProfilePicturePath = user.ProfilePicturePath
            };

            return Ok(new ApiResponse<UserProfileDto>
            {
                Success = true,
                Data = profile
            });
        }

        /// <summary>
        /// PUT /api/auth/profile
        /// Updates current user's profile (name, email, department)
        /// 
        /// JUSTIFICATION:
        /// - Profile page allows editing name and email
        /// - Password change handled separately for security
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Update allowed fields
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Email = request.Email ?? user.Email;
            user.Department = request.Department ?? user.Department;
            user.ProfilePicturePath = request.ProfilePicturePath ?? user.ProfilePicturePath;

            await _context.SaveChangesAsync();

            var profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = user.Department,
                Role = user.Role.ToString(),
                ProfilePicturePath = user.ProfilePicturePath
            };

            return Ok(new ApiResponse<UserProfileDto>
            {
                Success = true,
                Message = "Profile updated successfully",
                Data = profile
            });
        }

        /// <summary>
        /// PUT /api/auth/change-password
        /// Changes password for current user
        /// 
        /// SECURITY CONSIDERATIONS:
        /// - Requires current password for verification
        /// - Never return sensitive data
        /// - Log password change for audit trail
        /// </summary>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });
            }

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Incorrect current password" });
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Passwords do not match" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Password changed successfully"
            });
        }

        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "No file uploaded" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/{fileName}";

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "File uploaded successfully",
                Data = relativePath
            });
        }

        /// <summary>
        /// POST /api/auth/refresh
        /// Refreshes JWT token (if implementation uses refresh tokens)
        /// 
        /// JUSTIFICATION:
        /// - Long-lived JWT prevents re-login on every page load
        /// - Refresh token validates legitimacy before issuing new JWT
        /// </summary>
        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            // TODO: Validate refresh token
            // TODO: Issue new access token
            await Task.CompletedTask;

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Token refreshed"
            });
        }

        /// <summary>
        /// POST /api/auth/logout
        /// Logs out user (if needed for invalidation)
        /// 
        /// NOTE: With JWT, logout is client-side (token deletion)
        /// This endpoint could be used to:
        /// - Blacklist tokens (if using token blacklist strategy)
        /// - Log logout event for audit
        /// - Invalidate refresh tokens
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Optional: blacklist token, log event
            await Task.CompletedTask;

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logged out successfully"
            });
        }

        // Helper methods
        private bool VerifyPassword(string plainPassword, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(plainPassword, hash);
            }
            catch
            {
                // If the stored hash is not a valid BCrypt hash (e.g. plain text from old seed),
                // fall back to direct comparison, then return false
                return plainPassword == hash;
            }
        }
    }
}
