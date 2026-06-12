namespace AutoPageTurner.Models;

public class WindowItem
{
    public string Title { get; set; } = "";

    public string ProcessName { get; set; } = "";

    public IntPtr Handle { get; set; }

    public override string ToString()
    {
        return Title;
    }
}
