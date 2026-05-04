using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SendGrid.Helpers.Errors.Model;

namespace Tamkeen.Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new BadRequestException("Invalid image format");

            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("Image size must be less than 5MB");

            // Ex: wwwroot/uploads/tickets/
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", folder);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // return the relative URL
            return $"/uploads/{folder}/{fileName}";
        }
        public async Task<string> SaveIdCard(IFormFile file, string side)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file");

            // ── WebRootPath fallback ──────────────────────────────────
            // WebRootPath is null when no wwwroot folder exists or
            // UseStaticFiles() hasn't been configured yet.
            // We fall back to ContentRootPath/wwwroot and create it.
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(webRoot);
            }
            // ─────────────────────────────────────────────────────────

            var folder = Path.Combine(webRoot, "images", "id-cards");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}_{side}{ext}";
            var filePath = Path.Combine(folder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/id-cards/{fileName}";
        }

        public void DeleteImage(string url)
        {
            if (string.IsNullOrEmpty(url)) return;

            var filePath = Path.Combine(_env.WebRootPath, url.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    
}
}
