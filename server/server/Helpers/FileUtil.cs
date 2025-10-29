using Microsoft.AspNetCore.Http.Metadata;
using System.Security.AccessControl;

namespace server.Helpers
{
    public class FileHelper
    {

        private readonly IWebHostEnvironment _env;

        public FileHelper(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Uloží soubor do cílové složky pod wwwroot.
        /// </summary>
        /// <param name="folder">Cílová složka (např. "avatars")</param>
        /// <param name="file">Soubor k uložení</param>
        /// <returns>Relativní cesta k uloženému souboru (např. /avatars/xyz.png)</returns>
        public async Task<string> SaveFileAsync(string folder, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Soubor není validní.");

            var uploadsDir = Path.Combine(_env.WebRootPath, folder);
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var targetPath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(targetPath, FileMode.Create))
                await file.CopyToAsync(stream);

            return $"/{folder}/{fileName}";
        }
    }
}
