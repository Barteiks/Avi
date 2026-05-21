using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Provider;
using Avi.Services;
using Environment = Android.OS.Environment;
using Application = Android.App.Application;
using Uri = Android.Net.Uri;

namespace Avi.Platforms.Android
{
    public class AndroidPermissionManager : IPlatformPermissionManager
    {
        private TaskCompletionSource<bool>? _storageTcs;
        public Task<PermissionStatus> CheckAsync<TPermission>()
            where TPermission : Permissions.BasePermission, new()
        {
            return Permissions.CheckStatusAsync<TPermission>();
        }

        public Task<PermissionStatus> RequestAsync<TPermission>()
            where TPermission : Permissions.BasePermission, new()
        {
            return Permissions.RequestAsync<TPermission>();
        }
        public Task<bool> HasSpecialAsync(SpecialPermission permission)
        {
            return permission switch
            {
                SpecialPermission.AllFilesAccess =>
                    Task.FromResult(Environment.IsExternalStorageManager),

                _ => Task.FromResult(false)
            };
        }

        public Task RequestSpecialAsync(SpecialPermission permission)
        {
            switch (permission)
            {
                case SpecialPermission.AllFilesAccess:
                    {
                        // Zmiana na ActionManageAppAllFilesAccessPermission
                        var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
                        intent.SetData(Uri.Parse($"package:{Application.Context.PackageName}"));

                        intent.AddFlags(ActivityFlags.NewTask);
                        Application.Context.StartActivity(intent);
                        break;
                    }
            }

            return Task.CompletedTask;
        }
        public async Task<bool> RequestAndWaitForStoragePermissionAsync()
        {
            if (await HasSpecialAsync(SpecialPermission.AllFilesAccess))
                return true;

            _storageTcs = new TaskCompletionSource<bool>();

            await RequestSpecialAsync(SpecialPermission.AllFilesAccess);

            // Zwracamy wynik działania Taska (true lub false)
            return await _storageTcs.Task;
        }
        public void OnAppResumed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_storageTcs != null)
                {
                    // Sprawdzamy stan faktyczny
                    bool isGranted = await HasSpecialAsync(SpecialPermission.AllFilesAccess);

                    // ZAWSZE ustawiamy wynik, przekazując true ALBO false, 
                    // dzięki czemu aplikacja nigdy się nie zawiesi
                    _storageTcs.TrySetResult(isGranted);
                }
            });
        }
    }
}