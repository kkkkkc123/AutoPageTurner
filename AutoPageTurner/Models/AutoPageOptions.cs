namespace AutoPageTurner.Models;

public sealed class AutoPageOptions
{
    public required IntPtr WindowHandle { get; init; }

    public int Interval { get; init; }

    public bool UseRandomInterval { get; init; }

    public int MinInterval { get; init; }

    public int MaxInterval { get; init; }

    public bool EnableAutoClick { get; init; }

    public int ClickX { get; init; }

    public int ClickY { get; init; }

    public int ClickDelay { get; init; }

    public bool EnableClickDrift { get; init; }

    public int ClickDriftRadius { get; init; }

    public string PageAction { get; init; } = "PageDown";

    public int WheelX { get; init; } = 1000;

    public int WheelY { get; init; } = 500;

    public bool EnableWheelDrift { get; init; }

    public int WheelDriftRadius { get; init; } = 10;
}
