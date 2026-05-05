using Avi.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Diagnostics;

namespace Avi
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
#if ANDROID
            builder.Services.AddSingleton<IPlatformPathService, Platforms.Android.PlatformPathServiceAndroid>();
#elif WINDOWS
            builder.Services.AddSingleton<IPlatformPathService, Platforms.Windows.PlatformPathServiceWindows>();
#endif
            builder.Services.AddSingleton<Managers.ISpeechManager, Managers.SpeechManager>();
            builder.Services.AddSingleton<Managers.LlamaManager>();
            builder.Services.AddSingleton<TaskManager>();
            builder.Services.AddSingleton<MainPage>();


            builder.Services.AddSingleton<IFileService, FileService>();
            var app = builder.Build();
            var pathService = app.Services.GetRequiredService<IPlatformPathService>();
            AppLogger.Logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AppLogger");
            AppLogger.Init(pathService);

            AppLogger.Info("AppLogger initialized");
            return app;
        }
    }
}
