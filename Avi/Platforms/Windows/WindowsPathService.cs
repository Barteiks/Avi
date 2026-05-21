using Avi.Services;

namespace Avi.Platforms.Windows
{
    public class PlatformPathServiceWindows : IPlatformPathService
    {
        private readonly string _mainFolder = FileSystem.AppDataDirectory;
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
        public void ensureCreated()
        {
            GetModelDirectory();
            GetLogsDirectory();
        }
    }
}