using Amazon.DynamoDBv2.DocumentModel;
using NUnit.Framework;
using System;
using System.IO;
using Moq;
using FileShare.Models;

namespace FileShare.Tests.Models
{
    [TestFixture]
    public class FileShareModelTests
    {
        private Mock<IFormFile> _mockFile= new Mock<IFormFile>();

        private const long _length = 1024;
        private const int _duration = 10;
        private const string _testUrl = "https://example.com/testfile";

        [SetUp]
        public void Setup()
        {
            _mockFile = new Mock<IFormFile>();
            _mockFile.Setup(f => f.FileName).Returns("testfile.txt");
            _mockFile.Setup(f => f.Length).Returns(_length); // Пример длины файла
        }

        [Test]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var model = new FileShareModel(_mockFile.Object, _duration);

            // Assert
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Name, Is.EqualTo("testfile"));
            Assert.That(model.Extension, Is.EqualTo(".txt"));
            Assert.That(model.Duration, Is.EqualTo(_duration));
            Assert.That(model.Length, Is.EqualTo(_length));
            Assert.That(model.UniqueFileName, Is.Not.Null);
            Assert.That(model.Timestamp, Is.Not.Null);
        }

        [Test]
        public void ApplyUrl_ShouldSetUrlAndGenerateQRCodeSvg()
        {
            // Arrange
            var model = new FileShareModel(_mockFile.Object, _duration);
            model.ApplyUrl(_testUrl);

            // Assert
            Assert.That(_testUrl, Is.EqualTo(model.Url));
            Assert.That(model.QRCodeSvg, Is.Not.Null);
        }

        [Test]
        public void GetDocument_ShouldReturnCorrectDocument()
        {
            // Arrange
            var model = new FileShareModel(_mockFile.Object, _duration);
            Document d = model.GetDocument();

            // Assert
            Assert.That(d, Is.Not.Null);
            if (d != null)
            {
                Assert.That(model.Id, Is.EqualTo(d[nameof(model.Id)].AsGuid()));
                Assert.That(model.Name, Is.EqualTo(d[nameof(model.Name)].AsString()));
                Assert.That(model.Extension, Is.EqualTo(d[nameof(model.Extension)].AsString()));
                Assert.That(model.Date, Is.EqualTo(d[nameof(model.Date)].AsDateTime()));
                Assert.That(model.Timestamp, Is.EqualTo(d[nameof(model.Timestamp)].AsString()));
                Assert.That(model.UniqueFileName, Is.EqualTo(d[nameof(model.UniqueFileName)].AsString()));
                Assert.That(model.Length, Is.EqualTo(d[nameof(model.Length)].AsLong()));
                Assert.That(model.Duration, Is.EqualTo(d[nameof(model.Duration)].AsInt()));
                Assert.That(model.Url, Is.EqualTo(d[nameof(model.Url)].AsString()));
            }
        }

        [Test]
        public void ToString_ShouldReturnCorrectString()
        {
            // Arrange
            var model = new FileShareModel(_mockFile.Object, _duration);
            string result = model.ToString();

            // Assert
            Assert.That(result.Contains($"Id:{model.Id}"), Is.True);
            Assert.That(result.Contains($"Name:{model.Name}"), Is.True);
            Assert.That(result.Contains($"Length:{model.Length}"), Is.True);
            Assert.That(result.Contains($"Date:{model.Date}"), Is.True);
            Assert.That(result.Contains($"Duration:{model.Duration}"), Is.True);
        }
    }
}