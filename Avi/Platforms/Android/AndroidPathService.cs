
using Avi.Services;
using Environment = Android.OS.Environment;
using Application = Android.App.Application;
namespace Avi.Platforms.Android
{
    public class PlatformPathServiceAndroid : IPlatformPathService
    {
        private readonly string _mainFolder;
        public PlatformPathServiceAndroid()
        {
            _mainFolder = Path.Combine(Environment.ExternalStorageDirectory.AbsolutePath);
        }

        public string GetModelDirectory()
        {
            var path = Path.Combine(_mainFolder, "Avi", "Models");
            Directory.CreateDirectory(path);
            return path;
        }

        public string GetLogsDirectory()
        {
            var path = Path.Combine(_mainFolder, "Avi", "Logs");
            Directory.CreateDirectory(path);
            return path;

        }
    }
}
