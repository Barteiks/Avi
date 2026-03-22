using System.IO;

namespace Avi.Services
{
    public static class AppDataDirectories
    {
        public static readonly string Root =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Avi"
            );

        public static readonly string Sessions =
            Path.Combine(Root, "sessions");

        public static readonly string History =
            Path.Combine(Root, "history");

        public static readonly string Logs =
            Path.Combine(Root, "logs");

        /// <summary>
        /// Tworzy wszystkie wymagane katalogi aplikacji
        /// </summary>
        public static void EnsureCreated()
        {
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(Sessions);
            Directory.CreateDirectory(History);
            Directory.CreateDirectory(Logs);
        }
    }
}