using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using visionguard.Data;
using visionguard.DTOs;
using visionguard.Models;

namespace visionguard.Controllers
{
    /// <summary>
    /// Users Controller — manages system user accounts
    /// Supervisor-only for management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SAFETY_SUPERVISOR")]
    public class UsersController : ControllerBase
    {
        private readonly VisionGuardDbContext _context;

        public UsersController(VisionGuardDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET /api/users — Paginated list of all system users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100,
            [FromQuery] string? roleFilter = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(roleFilter) && Enum.TryParse<UserRole>(roleFilter, true, out var role))
                query = query.Where(u => u.Role == role);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    EmployeeId = u.EmployeeId,
                    Department = u.Department,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return Ok(new PagedResponse<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        /// <summary>
        /// GET /api/users/{id} — Get single user
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    EmployeeId = u.EmployeeId,
                    Department = u.Department,
                    Role = u.Role.ToString(),
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

            return Ok(new ApiResponse<UserDto> { Success = true, Data = user });
        }

        /// <summary>
        /// POST /api/users — Create new system user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Username already exists" });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Email already exists" });

            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid role" });

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmployeeId = request.EmployeeId,
                Department = request.Department,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                Department = user.Department,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                new ApiResponse<UserDto> { Success = true, Message = "User created successfully", Data = dto });
        }

        /// <summary>
        /// PUT /api/users/{id} — Update user details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;
            if (request.Email != null) user.Email = request.Email;
            if (request.Department != null) user.Department = request.Department;
            if (request.Role != null && Enum.TryParse<UserRole>(request.Role, true, out var role))
                user.Role = role;
            if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

            await _context.SaveChangesAsync();

            var dto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmployeeId = user.EmployeeId,
                Department = user.Department,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            return Ok(new ApiResponse<UserDto> { Success = true, Message = "User updated successfully", Data = dto });
        }

        /// <summary>
        /// DELETE /api/users/{id} — Hard-delete user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

            // Protect Supervisor users from deletion
            if (user.Role == UserRole.SAFETY_SUPERVISOR && user.Department == "Safety & Compliance")
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cannot delete Supervisor users." });
            }

            // Protect main HR user (HR-001) from deletion
            if (user.Username == "HR-001")
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cannot delete the main HR user (HR-001)." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object> { Success = true, Message = "User deleted successfully" });
        }

        /// <summary>
        /// POST /api/users/{id}/reset-password — Reset user password
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "User not found" });

            var tempPassword = "Reset1234!";
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Password reset to temporary password"
            });
        }
    }
}
