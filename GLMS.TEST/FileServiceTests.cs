using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;


namespace GLMS.TEST
{
    public class FileServiceTests
    {

        // Creates a mock IFormFile with a given filename and MIME type
        private static IFormFile MakeFakeFile(string fileName, string contentType,
            string content = "fake content")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.ContentType).Returns(contentType);
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken _) =>
                {
                    stream.Position = 0;
                    return stream.CopyToAsync(target);
                });
            return file.Object;
        }
        // Build a FileService with a temp directory as wwwroot
        private static FileService BuildService(out string tempRoot)
        {
            tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(tempRoot);
            var logger = Mock.Of<ILogger<FileService>>();
            return new FileService(env.Object, logger);
        }
        [Fact]
        public async Task SaveAgreementAsync_ValidPdf_SavesFileAndReturnsPath()
        {
            var svc = BuildService(out var root);
            var file = MakeFakeFile("agreement.pdf", "application/pdf");
            var (path, name) = await svc.SaveAgreementAsync(file);
            Assert.EndsWith(".pdf", path);
            Assert.Equal("agreement.pdf", name);
            // Verify the file physically exists on disk
            var fullPath = Path.Combine(root, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(fullPath));
        }
        [Fact]
        public async Task SaveAgreementAsync_ExeFile_ThrowsInvalidOperationException()
        {
            var svc = BuildService(out _);
            var file = MakeFakeFile("malware.exe", "application/octet-stream");
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.SaveAgreementAsync(file));
            Assert.Contains(".exe", ex.Message);
        }
        [Fact]
        public async Task SaveAgreementAsync_DocxFile_ThrowsInvalidOperationException()
        {
            var svc = BuildService(out _);
            var file = MakeFakeFile("contract.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.SaveAgreementAsync(file));
        }
        [Fact]
        public async Task SaveAgreementAsync_PdfExtensionButWrongMime_ThrowsInvalidOperationException()
        {
            // Attacker renames .exe to .pdf but MIME is still application/octet-stream
            var svc = BuildService(out _);
            var file = MakeFakeFile("notreally.pdf", "application/octet-stream");
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.SaveAgreementAsync(file));
        }
        [Fact]
        public async Task SaveAgreementAsync_NullFile_ThrowsInvalidOperationException()
        {
            var svc = BuildService(out _);
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.SaveAgreementAsync(null!));
        }
        [Fact]
        public async Task SaveAgreementAsync_TwoUploads_GenerateUniqueFileNames()
        {
            var svc = BuildService(out _);
            var file1 = MakeFakeFile("agreement.pdf", "application/pdf", "content A");
            var file2 = MakeFakeFile("agreement.pdf", "application/pdf", "content B");
            var (path1, _) = await svc.SaveAgreementAsync(file1);
            var (path2, _) = await svc.SaveAgreementAsync(file2);
            Assert.NotEqual(path1, path2);
        }
        [Fact]
        public void DeleteAgreement_ExistingFile_RemovesFromDisk()
        {
            var svc = BuildService(out var root);
            // Create a dummy file to delete
            var rel = "/uploads/agreements/dummy.pdf";
            var full = Path.Combine(root, "uploads", "agreements", "dummy.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, "dummy");
            svc.DeleteAgreement(rel);
            Assert.False(File.Exists(full));
        }
    }
}
