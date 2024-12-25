using Amazon.S3.Model;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Transfer;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using FileShare.Models;
using System.Drawing;
using FileShare.Factories;

namespace FileShare.Services
{
    public interface IFileShareService
    {
        Task<string> UploadFileAsync(Stream fileStream, FileShareModel model);
        Task SaveFileMetadataAsync(FileShareModel model);
    }

    public class FileShareService : IFileShareService
    {
        private readonly ILoggerService log;
        private IAmazonS3 _s3Client;
        private IAmazonDynamoDB _dynamoDBClient;
        private ITableFactory _tableFactory;
        private TransferUtility _transferUtility;

        private const string CONST_ShareFilesBucket = "share.files.bucket";
        private const string CONST_FileRecordsTable = "FileRecords";
        private const int CONST_TransferTreads = 10;
        private const long CONST_TransferPartMinSize = 5 * 1024 * 1024; // 5 Mb
        private TransferUtilityConfig transferConfig = new TransferUtilityConfig
        {
            ConcurrentServiceRequests = CONST_TransferTreads,
            MinSizeBeforePartUpload = CONST_TransferPartMinSize
        };

        public FileShareService(ILoggerService logger)
        {
            this.log = logger;

            log.Info($"Create credentails");
            var awsCredentials = new BasicAWSCredentials(Configuration.AwsConfig.AccessKey, Configuration.AwsConfig.SecretKey);
            log.Info($"Create S3 client");
            _s3Client = new AmazonS3Client(awsCredentials, Configuration.AwsConfig.Region);
            log.Info($"Create DynamoDB client");
            _dynamoDBClient = new AmazonDynamoDBClient(Configuration.AwsConfig.Region);

            _tableFactory = new TableFactory(_dynamoDBClient);
            _transferUtility = new TransferUtility(_s3Client, transferConfig);
        }

        public void TestInit(TransferUtility transferUtility)
        {
            _transferUtility = transferUtility;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, FileShareModel model)
        {
            var tuRequest = new TransferUtilityUploadRequest
            {
                BucketName = CONST_ShareFilesBucket,
                Key = model.UniqueFileName,
                InputStream = fileStream,
                //ContentType = file.ContentType
                ContentType = "application/octet-stream" // unknown file type
            };

            /* old
            TransferUtility + UploadAsync
            более надежный, чем _s3Client.PutObjectAsync(putRequest)
            с возможностью автоматического разбиения файла на части и параллельной загрузкой

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = "application/octet-stream"
            };
            var response = await _s3Client.PutObjectAsync(putRequest);*/

            using (log.Scoped(nameof(UploadFileAsync)))
            {
                try
                {
                    log.Info($"Upload file to S3: upload [{model}]");
                    await _transferUtility.UploadAsync(tuRequest);
                    log.Info($"Upload file to S3: upload OK [{model}]");

                    //throw new Exception("test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message test error message ");

                    var url = GeneratePreSignedURL(model);
                    return url;
                }
                catch (AmazonS3Exception s3Ex)
                {
                    log.Error("Error encountered in S3 when writing an object", s3Ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    log.Error("Unknown encountered on server when writing an object", ex.Message);
                    throw;
                }
            }
        }

        public async Task SaveFileMetadataAsync(FileShareModel model)
        {
            using (log.Scoped(nameof(SaveFileMetadataAsync)))
            {
                log.Info($"Save metadata in DynamoDB: data");
                var fileMetadata = model.GetDocument();
                log.Info($"Save metadata in DynamoDB: load table {CONST_FileRecordsTable}");
                var table = _tableFactory.Create(CONST_FileRecordsTable);
                log.Info($"Save metadata in DynamoDB: put item [{model}]");
                await table.PutItemAsync(fileMetadata);
                log.Info($"Save metadata in DynamoDB: put item OK [{model}]");
            }
        }

        public async Task<(Stream, string)> DownloadFileAsync(string fileName)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = CONST_ShareFilesBucket,
                Key = fileName
            };

            using (var response = await _s3Client.GetObjectAsync(getRequest))
            {
                var stream = response.ResponseStream;
                var contentType = response.Headers["Content-Type"];
                return (stream, contentType);
            }
        }

        public string GeneratePreSignedURL(FileShareModel model)
        {
            using (log.Scoped(nameof(GeneratePreSignedURL)))
            {
                log.Info($"Generate URL: params");
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = CONST_ShareFilesBucket,
                    Key = model.UniqueFileName,
                    Expires = DateTime.UtcNow.AddMinutes(model.Duration)
                };
                log.Info($"Generate URL: generate");
                var url = _s3Client.GetPreSignedURL(request);
                log.Info($"Generate URL: generate OK [Id:{model.Id} Url:{url}]");

                return url;
            }
        }
    }
}
