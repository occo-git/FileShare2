using Amazon.S3;
using FileShare.Controllers.Home;
using FileShare.Models;
using FileShare.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FileShare.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IFileShareService> _mockFileShareService = new Mock<IFileShareService>();
        private Mock<IFormFile> _mockFile = new Mock<IFormFile>();

        private const int _duration = 10;
        private const string _fileName = "file.txt";
        private const string _fileUrl = "http://example.com/file";

        [SetUp]
        public void Setup()
        {
            _mockFileShareService = new Mock<IFileShareService>();

            _mockFile = new Mock<IFormFile>(); 
            _mockFile.Setup(f => f.FileName).Returns(_fileName);
            _mockFile.Setup(f => f.Length).Returns(1024); // File length
            _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);
        }

        [Test]
        public void HomeView_ShouldReturnView()
        {
            // Arrange
            var controller = new HomeController(_mockFileShareService.Object);

            // Act
            var result = controller.HomeView();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Upload_FileIsNull_ReturnsBadRequest()
        {
            // Arrange
            var controller = new HomeController(_mockFileShareService.Object);
            IFormFile file = null;

            // Act
            var result = await controller.Upload(file, _duration) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            { 
                Assert.That(result.StatusCode, Is.EqualTo(400));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.Message, Is.EqualTo("File is empty or not provided."));
                }
            }
        }

        [Test]
        public async Task Upload_FileIsTooBig_ReturnsBadRequest()
        {
            // Arrange
            Configuration.TestInit();
            _mockFile.Setup(f => f.Length).Returns(Configuration.MainConfig.MaxFileSize + 1);
            var controller = new HomeController(_mockFileShareService.Object);

            // Act
            var result = await controller.Upload(_mockFile.Object, _duration) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(400));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.Message, Is.EqualTo("File is too big."));
                }
            }
        }

        [Test]
        public async Task Upload_ValidFile_ReturnsOk()
        {
            // Arrange
            Configuration.TestInit();
            _mockFileShareService.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<FileShareModel>())).ReturnsAsync(_fileUrl);
            _mockFileShareService.Setup(s => s.SaveFileMetadataAsync(It.IsAny<FileShareModel>())).Returns(Task.CompletedTask);

            var controller = new HomeController(_mockFileShareService.Object);

            // Act
            var result = await controller.Upload(_mockFile.Object, _duration) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(200));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.Message, Is.EqualTo("File successfully uploaded: "));
                    Assert.That(val.Url, Is.EqualTo(_fileUrl));
                }
            }
        }

        [Test]
        public async Task Upload_AmazonS3Exception_ReturnsBadRequest()
        {
            // Arrange
            Configuration.TestInit();
            _mockFileShareService
                .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<FileShareModel>()))
                .ThrowsAsync(new AmazonS3Exception("S3 error"));

            var controller = new HomeController(_mockFileShareService.Object);

            // Act
            var result = await controller.Upload(_mockFile.Object, _duration) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(400));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.Message, Is.EqualTo("Error uploading to S3: S3 error"));
                }
            }
        }

        [Test]
        public async Task Upload_UnexpectedException_ReturnsBadRequest()
        {
            // Arrange
            Configuration.TestInit();
            _mockFileShareService
                .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<FileShareModel>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            var controller = new HomeController(_mockFileShareService.Object);

            // Act
            var result = await controller.Upload(_mockFile.Object, _duration) as BadRequestObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(400));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.Message, Is.EqualTo("Unexpected error: Unexpected error"));
                }
            }
        }
    }
}