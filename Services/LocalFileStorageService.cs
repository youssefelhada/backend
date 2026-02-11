namespace visionguard.Services
{
    /// <summary>
    /// Local file system implementation of IFileStorageService.
    /// Stores files in wwwroot/uploads/{folder}/.
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<LocalFileStorageService> _logger;
        private const string UploadsFolder = "uploads";

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            ILogger<LocalFileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Save file to local storage: wwwroot/uploads/{folder}/{uniqueFileName}.
        /// </summary>
        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("File is empty or null.");
                }

                // Validate file type (images only)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException($"Invalid file type. Allowed: {string.Join(", ", allowedExtensions)}");
                }

                // Validate file size (max 5MB)
                const long maxFileSize = 5 * 1024 * 1024; // 5MB
                if (file.Length > maxFileSize)
                {
                    throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)}MB.");
                }

                // Generate unique file name
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";

                // Create directory path
                var uploadsPath = Path.Combine(_environment.WebRootPath, UploadsFolder, folder);
                
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Full file path
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path
                var relativePath = $"/{UploadsFolder}/{folder}/{uniqueFileName}";
                
                _logger.LogInformation("File saved successfully: {FilePath}", relativePath);
                
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file to local storage");
                throw;
            }
        }

        /// <summary>
        /// Delete file from local storage.
        /// </summary>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                // Remove leading slash if present
                var relativePath = filePath.TrimStart('/');

                // Full file path
                var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                    return true;
                }

                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from local storage: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// Get full URL for a file.
        /// For local storage, this returns the relative path (served by static files middleware).
        /// </summary>
        public string GetFileUrl(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            // Ensure path starts with /
            return relativePath.StartsWith("/") ? relativePath : $"/{relativePath}";
        }
    }
}
