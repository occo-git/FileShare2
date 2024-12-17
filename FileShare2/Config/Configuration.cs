using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using System.Text.Json;

public static class Configuration
{
    public static MainConfiguration MainConfig { get; private set; }
    public static AwsConfiguration AwsConfig { get; private set; }
    public static BuildConfiguration BuildConfig { get; private set; }

    private const string CONST_AccessKey = "AWS_ACCESS_KEY_ID";
    private const string CONST_SecretAccessKey = "AWS_SECRET_ACCESS_KEY";
    private const string CONST_Region = "AWS_REGION";
    private const string CONST_MaxFileSize = "MAX_FILE_SIZE";
    private const string CONST_PixelsPerModule = "PIXELS_PER_MODULE";
    private const string CONST_BuildConfigFilePath = @"build\build_config.json";

    public static void Init(this IConfiguration configuration)
    {
        if (MainConfig == null)
        {
            var maxFileSize = configuration.GetValue<long>(CONST_MaxFileSize);
            var pixelsPerModule = configuration.GetValue<int>(CONST_PixelsPerModule);
            MainConfig = new MainConfiguration(maxFileSize, pixelsPerModule);
        }
        if (AwsConfig == null)
        {
            var awsAccessKey = configuration[CONST_AccessKey] ?? string.Empty;
            var awsSecretKey = configuration[CONST_SecretAccessKey] ?? string.Empty;
            var awsRegion = configuration[CONST_Region] ?? string.Empty;
            AwsConfig = new AwsConfiguration(awsAccessKey, awsSecretKey, awsRegion);
        }
        if (BuildConfig == null)
        {
            var jsonString = File.ReadAllText(CONST_BuildConfigFilePath);
            var config = JsonSerializer.Deserialize<BuildConfiguration>(jsonString);
            BuildConfig = config ?? new BuildConfiguration();
         }
    }

    public static void TestInit()
    {
        if (MainConfig == null)
            MainConfig = new MainConfiguration(maxFileSize: 104857600, pixelsPerModule: 4);

        if (AwsConfig == null)
            AwsConfig = new AwsConfiguration(accessKey: "test_access_key", secretKey: "test_secret_key");

        if (BuildConfig == null)
            BuildConfig = new BuildConfiguration() { build_docker = new BuildDockerConfiguration() };
    }

    public static IServiceCollection ConfigureLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var loggingConfig = configuration.GetSection("Logging").Get<LoggingConfig>();

        if (loggingConfig != null)
            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                builder.AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext() // добавляет контекст к каждой записи
                    .WriteTo.Console(outputTemplate: loggingConfig.ConsoleTemplate)             // Запись в консоль             
                    .WriteTo.File(                                                              // Запись в файл
                        Path.Combine(AppContext.BaseDirectory, loggingConfig.LogFilePath),          // путь к файлу
                        outputTemplate: loggingConfig.FileTemplate,                                 // шаблон сообщения
                        rollingInterval: RollingInterval.Day,                                       // новый файл каждый день
                        fileSizeLimitBytes: loggingConfig.FileSizeLimitBytes,                       // максимальный размер одного файла
                        retainedFileTimeLimit: TimeSpan.FromDays(3),                                // хранить файлы последние 3 дня
                        rollOnFileSizeLimit: true)
                    .WriteTo.AmazonCloudWatch(                                                  // Запись в AWS Cloud Watch Logs
                            loggingConfig.AwsLogGroup,                                              // группа логов
                            loggingConfig.AwsStreamPrefix,                                          // префикс потока
                            batchSizeLimit: 100,
                            queueSizeLimit: 10000,
                            batchUploadPeriodInSeconds: 10,
                            createLogGroup: true,
                            maxRetryAttempts: 3)

                    //.WriteTo.S3("your-s3-bucket-name", "logs/{Date}/log.txt",
                    //    new S3SinkOptions
                    //    {
                    //        MinimumLogEventLevel = Serilog.Events.LogEventLevel.Information
                    //    })
                    .CreateLogger());
            });

        return services;
    }
}