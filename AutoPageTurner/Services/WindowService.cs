using System.Text;
using AutoPageTurner.Helpers;
using AutoPageTurner.Models;

namespace AutoPageTurner.Services;

public class WindowService
{
    public List<WindowItem> GetWindows()
    {
        List<WindowItem> windows = [];

        Win32.EnumWindows((hWnd, lParam) =>
        {
            if (!Win32.IsWindowVisible(hWnd))
                return true;

            StringBuilder sb = new(256);

            Win32.GetWindowText(
                hWnd,
                sb,
                sb.Capacity);

            string title = sb.ToString();

            if (!string.IsNullOrWhiteSpace(title))
            {
                windows.Add(new WindowItem
                {
                    Title = title,
                    Handle = hWnd
                });
            }

            return true;

        }, IntPtr.Zero);

        return windows
            .OrderBy(x => x.Title)
            .ToList();
    }
}