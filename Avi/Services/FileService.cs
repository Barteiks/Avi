namespace Avi.Services
{
    public interface IFileService
    {
        Task<string> ReadAssetTextAsync(string fileName);

        string GetAppDataFilePath(string fileName);

        string GetDefaultModelDirectory();

        void EnsureDirectoryExists(string path);
    }
    public class FileService : IFileService
    {
        private readonly IPlatformPathService _platformPaths;

        public FileService(IPlatformPathService platformPaths)
        {
            _platformPaths = platformPaths;
        }

        public async Task<string> ReadAssetTextAsync(string fileName)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public string GetAppDataFilePath(string fileName)
        {
            return Path.Combine(FileSystem.AppDataDirectory, fileName);
        }

        public string GetDefaultModelDirectory()
        {
            var dir = _platformPaths.GetModelDirectory();
            EnsureDirectoryExists(dir);
            return dir;
        }

        public void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
