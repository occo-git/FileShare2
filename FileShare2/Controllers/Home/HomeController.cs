using Amazon.S3;
using FileShare.Models;
using FileShare.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileShare.Controllers.Home
{
    /// <summary>
    /// Result of uploading a file
    /// </summary>
    public class FileUploadResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public string QRCode { get; private set; } = string.Empty;

        public FileUploadResult(string message)
        {
            Message = message;
        }

        public FileUploadResult(string message, string url, string qrCode)
        {
            Success = true;
            Message = message;
            Url = url;
            QRCode = qrCode;
        }
    }

    /// <summary>
    /// Main MVC-controller
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IFileShareService _fileShareService;

        public HomeController(IFileShareService fileShareService)
        {
            _fileShareService = fileShareService;
        }

        public ActionResult HomeView()
        {
            return View(); // return HomeView
        }

        [HttpPost]
        public async Task<IActionResult> Upload(
            IFormFile file,
            int duration)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    var message = "File is empty or not provided.";
                    return BadRequest(new FileUploadResult(message));
                }
                else if (!IsAllowedFileSize(file.Length))
                {
                    var message = "File is too big.";
                    return BadRequest(new FileUploadResult(message));
                }
                else
                {
                    // create the model with unique name
                    var model = new FileShareModel(file, duration);

                    using (var fileStream = new MemoryStream())
                    {
                        await file.CopyToAsync(fileStream);

                        var url = await _fileShareService.UploadFileAsync(fileStream, model); // Upload file to S3 
                        model.ApplyUrl(url);                       
                        await _fileShareService.SaveFileMetadataAsync(model); // Save metadata to DynamoDB

                        var message = "File successfully uploaded: ";
                        return Ok(new FileUploadResult(message, model.Url, model.QRCodeSvg));
                    }
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                // Logging S3 error
                var message = $"Error uploading to S3: {s3Ex.Message}";
                return BadRequest(new FileUploadResult(message));
            }
            catch (Exception ex)
            {
                // Logging error
                var message = $"Unexpected error: {ex.Message}";
                return BadRequest(new FileUploadResult(message));
            }
            finally
            {

            }
        }

        private bool IsAllowedFileSize(long size)
        {
            return size <= Configuration.MainConfig.MaxFileSize;
        }
    }
}

