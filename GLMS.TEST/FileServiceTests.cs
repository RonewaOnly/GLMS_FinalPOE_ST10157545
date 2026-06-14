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
        private static IFormFile MakeFile(string name, string mime, string body = "data")
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            var stream = new MemoryStream(bytes);
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(name);
            mock.Setup(f => f.ContentType).Returns(mime);
            mock.Setup(f => f.Length).Returns(bytes.Length);
            mock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns<Stream, CancellationToken>((s, _) => { stream.Position = 0; return stream.CopyToAsync(s); });
            return mock.Object;
        }


        private static FileService BuildSvc(out string root)
        {
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(root);
            var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns(root);
            return new FileService(env.Object, Mock.Of<ILogger<FileService>>());
        }


        [Fact]
        public async Task ValidPdf_Saves()
        {
            var svc = BuildSvc(out var root);
            var (path, name) = await svc.SaveAgreementAsync(MakeFile("a.pdf", "application/pdf"));
            Assert.EndsWith(".pdf", path);
            Assert.Equal("a.pdf", name);
            Assert.True(File.Exists(Path.Combine(root, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))));
        }
        [Fact]
        public async Task ExeFile_Throws() => await Assert.ThrowsAsync<InvalidOperationException>(() => BuildSvc(out _).SaveAgreementAsync(MakeFile("bad.exe", "application/octet-stream")));
        [Fact] public async Task DocxFile_Throws() => await Assert.ThrowsAsync<InvalidOperationException>(() => BuildSvc(out _).SaveAgreementAsync(MakeFile("doc.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")));
        [Fact] public async Task MimeSpoof_Throws() => await Assert.ThrowsAsync<InvalidOperationException>(() => BuildSvc(out _).SaveAgreementAsync(MakeFile("x.pdf", "application/octet-stream")));
        [Fact] public async Task NullFile_Throws() => await Assert.ThrowsAsync<InvalidOperationException>(() => BuildSvc(out _).SaveAgreementAsync(null!));
        [Fact]
        public async Task TwoUploads_UniqueNames()
        {
            var svc = BuildSvc(out _);
            var (p1, _) = await svc.SaveAgreementAsync(MakeFile("a.pdf", "application/pdf", "A"));
            var (p2, _) = await svc.SaveAgreementAsync(MakeFile("a.pdf", "application/pdf", "B"));
            Assert.NotEqual(p1, p2);
        }
        [Fact]
        public void Delete_RemovesFile()
        {
            var svc = BuildSvc(out var root);
            var rel = "/uploads/agreements/dummy.pdf";
            var full = Path.Combine(root, "uploads", "agreements", "dummy.pdf");
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, "dummy");
            svc.DeleteAgreement(rel);
            Assert.False(File.Exists(full));
        }
    }
}
