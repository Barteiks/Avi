using Android.App;
using Android.Content.PM;
using Android.OS;
using Avi.Services;

namespace Avi
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static IPlatformPermissionManager platformPermissionManager;
        protected override void OnResume()
        {
            base.OnResume();
            platformPermissionManager = IPlatformApplication.Current?.Services.GetService<IPlatformPermissionManager>();
            platformPermissionManager?.OnAppResumed();
        }
    }
}
