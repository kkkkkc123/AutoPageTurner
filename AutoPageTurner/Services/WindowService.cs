using System.Diagnostics;
using System.Text;
using AutoPageTurner.Helpers;
using AutoPageTurner.Models;

namespace AutoPageTurner.Services;

public sealed class WindowService
{
    public List<WindowItem> GetWindows()
    {
        List<WindowItem> windows = [];

        Win32.EnumWindows(
            (window, _) =>
            {
                if (!Win32.IsWindowVisible(
                        window))
                {
                    return true;
                }

                StringBuilder titleBuilder =
                    new(256);

                Win32.GetWindowText(
                    window,
                    titleBuilder,
                    titleBuilder.Capacity);

                string title =
                    titleBuilder.ToString();

                if (string.IsNullOrWhiteSpace(
                        title))
                {
                    return true;
                }

                string processName =
                    GetProcessName(
                        window);

                windows.Add(
                    new WindowItem
                    {
                        Title = title,
                        ProcessName = processName,
                        Handle = window
                    });

                return true;
            },
            IntPtr.Zero);

        return windows
            .OrderBy(
                item => item.Title)
            .ToList();
    }

    private static string GetProcessName(
        IntPtr window)
    {
        Win32.GetWindowThreadProcessId(
            window,
            out uint processId);

        try
        {
            using Process process =
                Process.GetProcessById(
                    (int)processId);

            return process.ProcessName;
        }
        catch
        {
            return "";
        }
    }
}
