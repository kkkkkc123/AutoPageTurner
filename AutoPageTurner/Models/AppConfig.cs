namespace AutoPageTurner.Models;

public class AppConfig
{
    public int Interval { get; set; } = 2000;

    public bool UseRandomInterval { get; set; } = true;

    public int MinInterval { get; set; } = 1500;

    public int MaxInterval { get; set; } = 3500;

    public bool EnableAutoClick { get; set; }

    public int ClickX { get; set; } = 1000;

    public int ClickY { get; set; } = 500;

    public int ClickDelay { get; set; } = 100;

    public bool EnableClickDrift { get; set; }

    public int ClickDriftRadius { get; set; } = 10;

    public string PageAction { get; set; } = "PageDown";

    public int WheelX { get; set; } = 1000;

    public int WheelY { get; set; } = 500;

    public bool EnableWheelDrift { get; set; }

    public int WheelDriftRadius { get; set; } = 10;

    public string TargetWindowTitle { get; set; } = "";

    public string TargetProcessName { get; set; } = "";
}
