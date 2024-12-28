using Serilog;
using System.Text;

namespace FileShare.Services
{
    public class SpeedLinkService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly ILoggerService _log;

        private const string CONST_Api = "https://speedlink.azurewebsites.net/";
        private const string CONST_ApiUrl = "https://speedlink.azurewebsites.net/?url=";

        public SpeedLinkService(ILoggerService logger)
        {
            _log = logger;
        }

        public async Task<string> CreateShortUrlAsync(string longUrl)
        {
            var jsonContent = new StringContent($"\"{longUrl}\"", Encoding.UTF8, "application/json");

            var response = await client.PostAsync(CONST_Api, jsonContent);
            if (response != null && response.IsSuccessStatusCode)
            {
                _log.Info($"Generate URL: short URL create");
                var shortUrl = await response.Content.ReadAsStringAsync();
                _log.Info($"Generate URL: short URL create OK - {shortUrl}");

                return $"{CONST_ApiUrl}{shortUrl}";
            }
            _log.Error($"Generate URL: hort URL create ERROR - {response?.StatusCode}");
            return string.Empty;
        }
    }
}
