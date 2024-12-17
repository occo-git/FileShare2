using Amazon.S3;
using Amazon.DynamoDBv2;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using FileShare.Models;
using FileShare.Services;
using Amazon.DynamoDBv2.DocumentModel;
using FileShare.Factories;
using Amazon.S3.Transfer;

namespace FileShare.Tests.Services
{
    [TestFixture]
    public class FileShareServiceTests
    {
        private Mock<ILoggerService> _mockLogger = new Mock<ILoggerService>();
        private Mock<IAmazonS3> _mockS3Client = new Mock<IAmazonS3>();
        private Mock<TransferUtility> _mockTransferUnity = new Mock<TransferUtility>();
        private Mock<IAmazonDynamoDB> _mockDynamoDBClient = new Mock<IAmazonDynamoDB>();
        private Mock<ITableFactory> _mockTableFactory = new Mock<ITableFactory>();
        private Mock<IFormFile> _mockFile = new Mock<IFormFile>();

        private const int _duration = 10;
        private const string _fileName = "file.txt";
        private const string _fileUrl = "http://example.com/file";

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILoggerService>();
            _mockS3Client = new Mock<IAmazonS3>();
            _mockDynamoDBClient = new Mock<IAmazonDynamoDB>();
            _mockTableFactory = new Mock<ITableFactory>();

            _mockFile = new Mock<IFormFile>();
            _mockFile.Setup(f => f.FileName).Returns(_fileName);
            _mockFile.Setup(f => f.Length).Returns(1024); // File length
        }

        //[Test]
        //public async Task UploadFileAsync_ShouldUploadFileAndReturnUrl()
        //{
        //    // Arrange
        //    Configuration.TestInit();
        //    var fileStream = new MemoryStream(new byte[] { 1, 2, 3 }); // Sample file data
        //    var model = new FileShareModel(_mockFile.Object, _duration);

        //    _mockTransferUnity.Setup(c => c.UploadAsync(It.IsAny<TransferUtilityUploadRequest>(), default)).Returns(Task.CompletedTask);

        //    var fileShareService = new FileShareService(_mockLogger.Object);
        //    fileShareService.TestInit(_mockTransferUnity.Object);

        //    // Act
        //    var url = await fileShareService.UploadFileAsync(fileStream, model);

        //    // Assert
        //    Assert.That(url, Is.Not.Null);
        //}

        //[Test]
        //public async Task SaveFileMetadataAsync_ShouldSaveMetadata()
        //{
        //    // Arrange
        //    Configuration.TestInit();
        //    var fileShareService = new FileShareService(_mockLogger.Object);

        //    var mockTable = new Mock<Table>();
        //    _mockTableFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(mockTable.Object);
        //    mockTable.Setup(t => t.PutItemAsync(It.IsAny<Document>(), default)).Returns(() => Task.CompletedTask);

        //    // Act
        //    await fileShareService.SaveFileMetadataAsync(It.IsAny<FileShareModel>());

        //    // Assert
        //    //mockTable.Verify(t => t.PutItemAsync(It.Is<Document>(d => d.Equals(fileMetadata)), default), Times.Once);
        //    mockTable.Verify(t => t.PutItemAsync(It.IsAny<Document>(), default), Times.Once); // PutItemAsync called only once
        //}
    }
}