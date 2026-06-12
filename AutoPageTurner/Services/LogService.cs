using System.IO;
using System.Text;

namespace AutoPageTurner.Services;

public static class LogService
{
    private const long MaximumLogSize =
        512 * 1024;

    private static readonly object SyncRoot =
        new();

    private static readonly string LogPath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "AutoPageTurner",
            "app.log");

    public static void Write(
        string message)
    {
        lock (SyncRoot)
        {
            try
            {
                string directory =
                    Path.GetDirectoryName(
                        LogPath)!;

                Directory.CreateDirectory(
                    directory);

                if (File.Exists(
                        LogPath) &&
                    new FileInfo(
                        LogPath).Length >
                    MaximumLogSize)
                {
                    File.Delete(
                        LogPath);
                }

                File.AppendAllText(
                    LogPath,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}",
                    new UTF8Encoding(false));
            }
            catch
            {
            }
        }
    }
}
