using Amazon.DynamoDBv2.DocumentModel;
using QRCoder;

namespace FileShare.Models
{
    public class FileShareModel
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public DateTime Date { get; private set; }
        public string Timestamp { get; private set; }
        public string UniqueFileName { get; private set; }
        public long Length { get; private set; }
        public int Duration { get; private set; } // minutes
        public string Url { get; private set; } = string.Empty;
        public string QRCodeSvg { get; private set; } = string.Empty;

        public FileShareModel(IFormFile file, int duration)
        {
            Id = Guid.NewGuid();
            Name = Path.GetFileNameWithoutExtension(file.FileName);
            Extension = Path.GetExtension(file.FileName);
            Date = DateTime.UtcNow;
            Timestamp = Date.ToString("yyyyMMddHHmmss");
            UniqueFileName = $"{Name}_{Id}_{Timestamp}{Extension}";
            Length = file.Length;
            Duration = duration;
        }

        public void ApplyUrl(string url)
        {
            Url = url;
            QRCodeSvg = GetQRCodeSvgString(url);
        }

        private string GetQRCodeSvgString(string preSignedUrl)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(preSignedUrl, QRCodeGenerator.ECCLevel.Q);
            SvgQRCode qrCode = new SvgQRCode(qrCodeData);
            string qrCodeSvgString = qrCode.GetGraphic(Configuration.MainConfig.PixelsPerModule);
            return qrCodeSvgString;
        }

        public Document GetDocument()
        {
            var d = new Document();
            d[nameof(Id)] = Id;
            d[nameof(Name)] = Name;
            d[nameof(Extension)] = Extension;
            d[nameof(Date)] = Date;
            d[nameof(Timestamp)] = Timestamp;
            d[nameof(UniqueFileName)] = UniqueFileName;
            d[nameof(Length)] = Length;
            d[nameof(Duration)] = Duration;
            d[nameof(Url)] = Url;
            return d;
        }

        public override string ToString()
        {
            return $"Id:{Id}, Name:{Name}, Length:{Length}, Date:{Date}, Duration:{Duration}";
        }
    }
}