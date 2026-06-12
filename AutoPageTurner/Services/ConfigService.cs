using System.IO;
using System.Text.Json;
using AutoPageTurner.Models;

namespace AutoPageTurner.Services;

public class ConfigService
{
    private readonly string filePath =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "config.json");

    public void Save(AppConfig config)
    {
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

        string json =
            File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<AppConfig>(json)
               ?? new AppConfig();
    }
}