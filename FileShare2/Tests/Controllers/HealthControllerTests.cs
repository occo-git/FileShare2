using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using FileShare.Controllers.Health;

namespace FileShare.Tests.Controllers
{
    [TestFixture]
    public class HealthControllerTests
    {
        [Test]
        public void GetMaxFileSize_ShouldReturnMaxFileSize()
        {
            // Arrange
            Configuration.TestInit();
            var controller = new HealthController();

            // Act
            var result = controller.GetMaxFileSize() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(200));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.MaxFileSize, Is.EqualTo(Configuration.MainConfig.MaxFileSize));
                }
            }
        }

        [Test]
        public void GetImageTag_ShouldReturnImageTag()
        {
            // Arrange
            Configuration.TestInit();
            var controller = new HealthController();

            // Act
            var result = controller.GetImageTag() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(200));
                Assert.That(result.Value, Is.Not.Null);
                if (result.Value != null)
                {
                    dynamic val = result.Value;
                    Assert.That(val.ImageTag, Is.EqualTo(Configuration.BuildConfig.build_docker.IMAGE_TAG));
                }
            }
        }

        [Test]
        public void Get_ShouldReturnHealthy()
        {
            // Arrange
            var controller = new HealthController();

            // Act
            var result = controller.Get() as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            if (result != null)
            {
                Assert.That(result.StatusCode, Is.EqualTo(200));
                Assert.That(result.Value, Is.EqualTo("Healthy"));
            }
        }
    }
}