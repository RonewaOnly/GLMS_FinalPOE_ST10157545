namespace GLMS.Web.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Validates and saves an uploaded PDF. Returns the saved relative path.
        /// Throws InvalidOperationException for invalid file types or empty files.
        /// </summary>
        Task<(string relativePath, string fileName)> SaveAgreementAsync(IFormFile file);
        /// <summary>Deletes a previously saved agreement file.</summary>
        void DeleteAgreement(string relativePath);
    }
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;
        // Only PDF is permitted per the business requirement
        private static readonly string[] AllowedExtensions = { ".pdf" };
        private static readonly string[] AllowedMimeTypes = { "application/pdf" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
                                                                // Folder relative to wwwroot where agreements are stored
        private const string StorageFolder = "uploads/agreements";
        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }
        public async Task<(string relativePath, string fileName)> SaveAgreementAsync(IFormFile file)
        {
            //  Validation 
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file was uploaded.");
            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException(
                    $"File exceeds the maximum allowed size of {MaxFileSizeBytes / 1024 / 1024} MB.");
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException(
                    $"Invalid file type '{extension}'. Only .pdf files are permitted.");
            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new InvalidOperationException(
                    $"Invalid MIME type '{file.ContentType}'. Only PDF files are permitted.");
            //  Persist 
            var uploadRoot = Path.Combine(_env.WebRootPath, StorageFolder);
            Directory.CreateDirectory(uploadRoot); // ensure folder exists
                                                   // Use a GUID to avoid file name collisions and path-traversal attacks
            var safeFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadRoot, safeFileName);
            var relativePath = $"/{StorageFolder}/{safeFileName}";
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            _logger.LogInformation("Agreement saved: {Path} (original: {Original})",
                relativePath, file.FileName);
            return (relativePath, file.FileName);
        }
        public void DeleteAgreement(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            // Convert relative web path → absolute OS path
            var fullPath = Path.Combine(_env.WebRootPath,
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Agreement deleted: {Path}", relativePath);
            }
        }
    }
}
