using System;
using System.Collections.Generic;
using System.Text;

namespace Avi.Services
{
    public interface IPlatformPermissionManager
    {
        Task<PermissionStatus> CheckAsync<TPermission>() where TPermission : Permissions.BasePermission, new();
        Task<PermissionStatus> RequestAsync<TPermission>() where TPermission : Permissions.BasePermission, new();
        Task RequestSpecialAsync(SpecialPermission permission);
        Task<bool> HasSpecialAsync(SpecialPermission permission);
        Task<bool> RequestAndWaitForStoragePermissionAsync();
        void OnAppResumed();
    }
    public enum SpecialPermission
    {
        AllFilesAccess,
        Overlay,
        Notifications
    }

}
