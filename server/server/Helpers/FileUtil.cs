using Microsoft.AspNetCore.Http.Metadata;
using System.Security.AccessControl;

namespace server.Helpers
{
    /// <summary>
    /// Utility service for handling file system operations, such as saving uploaded user content.
    /// Ensures secure file naming and directory management within the web root.
    /// </summary>
    public class FileHelper
    {
        private readonly IWebHostEnvironment _env;

        public FileHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Saves an uploaded file to a specified target folder within the wwwroot directory.
        /// Generates a unique GUID filename to prevent filename collisions and security risks.
        /// </summary>
        /// <param name="folder">Target sub-folder name (e.g., "avatars").</param>
        /// <param name="file">The uploaded file from the HTTP request.</param>
        /// <returns>A relative URL path to the stored file (e.g., "/avatars/guid.png").</returns>
        /// <exception cref="ArgumentException">Thrown when the provided file is null or empty.</exception>
        public async Task<string> SaveFileAsync(string folder, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("The provided file is invalid or empty.");
            }

            // Ensure the target directory exists within the web root (wwwroot)
            var uploadsDir = Path.Combine(_env.WebRootPath, folder);
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Generate a unique filename using GUID to prevent Overwrite attacks and Path Traversal
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var targetPath = Path.Combine(uploadsDir, fileName);

            // Use a stream to copy the file content to the target destination
            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Returns a relative path for frontend access and database storage
            return $"/{folder}/{fileName}";
        }
    }
}
