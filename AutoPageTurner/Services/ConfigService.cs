using System.IO;
using System.Text.Json;
using AutoPageTurner.Models;

namespace AutoPageTurner.Services;

public class ConfigService
{
    private readonly string filePath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "AutoPageTurner",
            "config.json");

    public void Save(AppConfig config)
    {
        string? directory =
            Path.GetDirectoryName(filePath);

        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        string json =
            JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        File.WriteAllText(
            filePath,
            json);
    }

    public AppConfig Load()
    {
        if (!File.Exists(filePath))
        {
            return new AppConfig();
        }

        try
        {
            string json =
                File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<AppConfig>(json)
                   ?? new AppConfig();
        }
        catch (JsonException)
        {
            return new AppConfig();
        }
        catch (IOException)
        {
            return new AppConfig();
        }
    }
}
