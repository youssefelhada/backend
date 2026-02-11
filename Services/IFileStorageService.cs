namespace visionguard.Services
{
    /// <summary>
    /// Abstraction for file storage operations.
    /// Supports local storage, cloud storage (S3, Azure Blob), etc.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Save an uploaded file to storage.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="folder">Target folder (e.g., "workers", "violations").</param>
        /// <returns>Relative path/URL to the saved file.</returns>
        Task<string> SaveFileAsync(IFormFile file, string folder);

        /// <summary>
        /// Delete a file from storage.
        /// </summary>
        /// <param name="filePath">Relative path/URL of the file to delete.</param>
        /// <returns>True if deleted successfully, false otherwise.</returns>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Get the full URL for a stored file.
        /// </summary>
        /// <param name="relativePath">Relative path of the file.</param>
        /// <returns>Full URL to access the file.</returns>
        string GetFileUrl(string relativePath);
    }
}
