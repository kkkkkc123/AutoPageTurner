namespace AutoPageTurner.Models;

public class AppConfig
{
    public int Interval { get; set; } = 2000;

    public bool UseRandomInterval { get; set; } = true;

    public int MinInterval { get; set; } = 1500;

    public int MaxInterval { get; set; } = 3500;

    public bool EnableAutoClick { get; set; }

    public int ClickX { get; set; }

    public int ClickY { get; set; }

    public int ClickDelay { get; set; } = 100;
}