using Avi.Functions;
using Avi.Services;
using Avi.UI;
using CommunityToolkit.Maui.Core;
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
                    fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSansSemibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeFluentIcons.ttf", "SegoeFluentIcons");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.UseMauiApp<App>().UseMauiCommunityToolkitCore();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
#if ANDROID
            builder.Services.AddSingleton<IPlatformPathService, Platforms.Android.PlatformPathServiceAndroid>();
            builder.Services.AddSingleton<IPlatformPermissionManager, Platforms.Android.AndroidPermissionManager>();
#elif WINDOWS
            builder.Services.AddSingleton<IPlatformPathService, Platforms.Windows.PlatformPathServiceWindows>();
            builder.Services.AddSingleton<IPlatformPermissionManager, Platforms.Windows.WindowsPermissionManager>();
#endif
            builder.Services.AddSingleton<Managers.ISpeechManager, Managers.SpeechManager>();
            builder.Services.AddSingleton<Managers.LlamaManager>();
            builder.Services.AddSingleton<TaskManager>();
            builder.Services.AddSingleton<SettingsView>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<IFileService, FileService>();
            var app = builder.Build();
            var permissionService = app.Services.GetRequiredService<IPlatformPermissionManager>();
            var pathService = app.Services.GetRequiredService<IPlatformPathService>();
            AppLogger.Logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AppLogger");
            AppLogger.Init(pathService, permissionService);

            AppLogger.Info("AppLogger initialized");
            return app;
        }
    }
}
