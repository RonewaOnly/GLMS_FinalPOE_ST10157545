namespace GLMS.API.Services { 

        public interface IFileService
        {
            Task<(string path, string fileName)> SaveAgreementAsync(IFormFile file);
            void DeleteAgreement(string relativePath);
        }

        public class FileService : IFileService
        {
            private readonly IWebHostEnvironment _env;
            private readonly ILogger<FileService> _logger;
            private static readonly string[] AllowedExt = { ".pdf" };
            private static readonly string[] AllowedMime = { "application/pdf" };
            private const long MaxBytes = 10 * 1024 * 1024;
            private const string Folder = "uploads/agreements";

            public FileService(IWebHostEnvironment env, ILogger<FileService> logger) { _env = env; _logger = logger; }

            public async Task<(string path, string fileName)> SaveAgreementAsync(IFormFile file)
            {
                if (file == null || file.Length == 0) throw new InvalidOperationException("No file uploaded.");
                if (file.Length > MaxBytes) throw new InvalidOperationException("File exceeds 10 MB limit.");
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExt.Contains(ext)) throw new InvalidOperationException($"Invalid file type '{ext}'. Only .pdf allowed.");
                if (!AllowedMime.Contains(file.ContentType.ToLowerInvariant())) throw new InvalidOperationException($"Invalid MIME type. Only application/pdf allowed.");
                var root = Path.Combine(_env.WebRootPath ?? "wwwroot", Folder);
                Directory.CreateDirectory(root);
                var safe = $"{Guid.NewGuid()}{ext}";
                var fullPath = Path.Combine(root, safe);
                var relPath = $"/{Folder}/{safe}";
                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
                _logger.LogInformation("Saved: {Path}", relPath);
                return (relPath, file.FileName);
            }

            public void DeleteAgreement(string relativePath)
            {
                if (string.IsNullOrWhiteSpace(relativePath)) return;
                var full = Path.Combine(_env.WebRootPath ?? "wwwroot", relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(full)) { File.Delete(full); _logger.LogInformation("Deleted: {Path}", relativePath); }
            }
        }
    

}
