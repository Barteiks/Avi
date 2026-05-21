using Avi.Services;

namespace Avi.Platforms.Windows
{
    public class WindowsPermissionManager : IPlatformPermissionManager
    {
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
        public Task RequestSpecialAsync(SpecialPermission permission)
        {
            // Windows nie ma takich permissionów
            return Task.CompletedTask;
        }

        public Task<bool> HasSpecialAsync(SpecialPermission permission)
        {
            // Windows traktujemy jako "zawsze OK" albo zależnie od twojej logiki
            return Task.FromResult(true);
        }
        public Task<bool> RequestAndWaitForStoragePermissionAsync()
        {
            // Windows nie wymaga specjalnego requestu dla storage, więc zwracamy od razu true
            return Task.FromResult(true);
        }
        public void OnAppResumed() { }
        
    }
}