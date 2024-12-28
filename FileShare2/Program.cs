using Amazon.S3;
using Microsoft.AspNetCore.Http.Features;
using FileShare.Services;
using Amazon.DynamoDBv2;
using FileShare.Factories;

namespace FileShare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Экземпляр WebApplicationBuilder, который используется для настройки служб и параметров приложения
            var builder = WebApplication.CreateBuilder(args);

            #region Configuration
            builder.Configuration.Init();
            #endregion

            #region Logging
            builder.Logging.ClearProviders(); // Удаляем все провайдеры логирования по умолчанию
            builder.Logging.AddConsole(); // Добавляем логирование в консоль
            builder.Logging.AddDebug(); // Добавляем логирование для отладки
            builder.Services.ConfigureLogging(builder.Configuration);
            builder.Services.AddSingleton<ILoggerService, LoggerService>();
            #endregion

            #region AWS        
            builder.Services.AddAWSService<IAmazonS3>(); // Регистрация IAmazonS3
            builder.Services.AddAWSService<IAmazonDynamoDB>(); // Регистрация DynamoDB
            #endregion

            #region CORS
            builder.Services.AddCors(options => // CORS
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
            #endregion

            #region Add services to the container
            builder.Services.AddScoped<IFileShareService, FileShareService>(); // Регистрирует сервис FileShare в контейнер зависимостей 
            builder.Services.AddSingleton<SpeedLinkService>(); // short url service       
            builder.Services.AddControllersWithViews(); // Добавляет поддержку контроллеров MVC с представлениями            
            builder.Services.AddControllers(); // Добавляет поддержку контроллеров API            
            builder.Services.AddEndpointsApiExplorer(); // Необходим для автоматического обнаружения конечных точек API            
            builder.Services.AddSwaggerGen(); // Добавляет поддержку Swagger для документирования вашего API
            //builder.Services.AddOpenApi(); // Swagger doc gen https://aka.ms/aspnet/openapi
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Configuration.MainConfig.MaxFileSize;
            });
            //builder.Services.ConfigureSwaggerGen(options =>
            //{
            //    options.OperationFilter<FileUploadOperation>(); // Register FileUploadOperation Filter
            //});
            #endregion

            #region Настройка Kestrel
            //app.Urls.Add("http://localhost:80"); // Настройка HTTP на порту 80
            //app.Urls.Add("https://localhost:443"); // Настройка HTTPS на порту 443, если нужно
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = Configuration.MainConfig.MaxFileSize;
                options.ListenAnyIP(80); // Слушает на порту 80
                //options.ListenAnyIP(443, listenOptions =>
                //{
                //    listenOptions.UseHttps("path/to/certificate.pfx", "your_certificate_password"); // HTTPS
                //});
            });
            #endregion

            var app = builder.Build(); // Экземпляр приложения
            // Получаем логгер
            var logger = app.Services.GetRequiredService<ILoggerService>();

            #region Настройка middleware (промежуточных обработчиков)
            if (app.Environment.IsDevelopment())
            {
                logger.Info("IsDevelopment=true");
                // Если приложение находится в режиме разработки, включается Swagger и его интерфейс пользователя для тестирования API
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.MapOpenApi(); // Swagger doc gen
                //app.UseSwaggerUI(c =>
                //{
                //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //    c.RoutePrefix = "swagger";
                //});

                app.UseDeveloperExceptionPage(); // Отображает страницу с ошибками в разработке
            }
            else
            {
                logger.Info("IsDevelopment=false");
                app.UseExceptionHandler("/Home/Error"); // Перенаправляет на страницу ошибки
                //app.UseHsts(); // Включает HSTS (HTTP Strict Transport Security) — веб-протокол безопасности
            }
            #endregion

            //app.UseHttpsRedirection(); // Перенаправляет HTTP-запросы на HTTPS
            app.UseStaticFiles(); // Позволяет обслуживать статические файлы (HTML, CSS, JavaScript, изображения и другие ресурсы, находящиеся в папке wwwroot (по умолчанию))
            
            #region Настройка маршрутов
            app.UseRouting(); // Настраивает маршрутизацию, позволяя определять, как запросы будут сопоставляться с конечными точками (например, контроллерами и действиями в MVC)            
            app.UseCors("AllowAll"); // Включение CORS  (after Routing)
            //app.UseAuthentication(); // Проверка аутентификации
            //app.UseAuthorization();  // Проверка авторизации (after UseAuthentication)

            //app.UseEndpoints(e => { });             
            app.MapControllers(); // Настраивает маршруты для контроллеров API
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=HomeView}/{id?}"); // Home и HomeView как значения по умолчанию
            //app.MapGet("/", () => Results.Redirect("/Home/HomeView")); // Перенаправление с корневого URL на домашнюю страницу            
            #endregion

            app.Run(); // Запускает приложение и начинает прослушивание HTTP-запросов
        }
    }
}