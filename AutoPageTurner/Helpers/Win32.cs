using System.Runtime.InteropServices;
using System.Text;

namespace AutoPageTurner.Helpers;

public static class Win32
{
    #region Window

    public delegate bool EnumWindowsProc(
        IntPtr hWnd,
        IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(
        EnumWindowsProc lpEnumFunc,
        IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(
        IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int GetWindowText(
        IntPtr hWnd,
        StringBuilder text,
        int count);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(
        IntPtr hWnd,
        out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool ScreenToClient(
        IntPtr hWnd,
        ref POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern IntPtr ChildWindowFromPointEx(
        IntPtr hWndParent,
        POINT point,
        uint flags);

    public const uint CWP_SKIPINVISIBLE = 0x0001;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    #endregion

    #region Keyboard

    [DllImport("user32.dll")]
    public static extern bool PostMessage(
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        IntPtr lParam);

    public const uint WM_KEYDOWN = 0x0100;

    public const uint WM_KEYUP = 0x0101;

    public const int VK_NEXT = 0x22; // PageDown

    #endregion

    #region Mouse

    public const uint WM_LBUTTONDOWN = 0x0201;

    public const uint WM_LBUTTONUP = 0x0202;

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(
        out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;

        public int Y;
    }

    #endregion
}
