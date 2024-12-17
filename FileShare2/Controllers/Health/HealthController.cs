using Microsoft.AspNetCore.Mvc;

namespace FileShare.Controllers.Health
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet("maxfilesize")]
        public IActionResult GetMaxFileSize()
        {
            return Ok(new { MaxFileSize = Configuration.MainConfig.MaxFileSize });
        }

        [HttpGet("imagetag")]
        public IActionResult GetImageTag()
        {
            return Ok(new { ImageTag = Configuration.BuildConfig.build_docker.IMAGE_TAG });
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Healthy");
        }
    }
}