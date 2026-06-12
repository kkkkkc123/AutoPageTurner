using System.IO;
using System.Text;
using System.Text.Json;
using AutoPageTurner.Models;

namespace AutoPageTurner.Services;

public sealed class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true
        };

    private readonly string filePath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "AutoPageTurner",
            "config.json");

    public string FilePath => filePath;

    public bool TrySave(
        AppConfig config,
        out string? error)
    {
        string temporaryPath =
            filePath + ".tmp";

        try
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(
                    filePath)!);

            string json =
                JsonSerializer.Serialize(
                    config,
                    JsonOptions);

            File.WriteAllText(
                temporaryPath,
                json,
                new UTF8Encoding(false));

            File.Move(
                temporaryPath,
                filePath,
                true);

            error = null;
            return true;
        }
        catch (Exception exception)
            when (exception is IOException or
                  UnauthorizedAccessException)
        {
            TryDelete(
                temporaryPath);

            error =
                exception.Message;
            return false;
        }
    }

    public AppConfig Load()
    {
        if (!File.Exists(
                filePath))
        {
            return new AppConfig();
        }

        try
        {
            string json =
                File.ReadAllText(
                    filePath);

            return JsonSerializer.Deserialize<AppConfig>(
                       json)
                   ?? new AppConfig();
        }
        catch (Exception exception)
            when (exception is JsonException or
                  IOException or
                  UnauthorizedAccessException)
        {
            return new AppConfig();
        }
    }

    public bool TryReset(
        out string? error)
    {
        try
        {
            if (File.Exists(
                    filePath))
            {
                File.Delete(
                    filePath);
            }

            error = null;
            return true;
        }
        catch (Exception exception)
            when (exception is IOException or
                  UnauthorizedAccessException)
        {
            error =
                exception.Message;
            return false;
        }
    }

    private static void TryDelete(
        string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }
}
