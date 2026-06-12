using System.Threading;
using System.Windows.Threading;
using AutoPageTurner.Helpers;

namespace AutoPageTurner.Services;

public class AutoPageService
{
    private readonly DispatcherTimer timer;

    private IntPtr currentWindow;

    private readonly Random random = new();

    private bool useRandom;

    private int minInterval;

    private int maxInterval;

    private bool enableAutoClick;

    private int clickX;

    private int clickY;

    private int clickDelay;

    public bool IsRunning => timer.IsEnabled;

    public AutoPageService()
    {
        timer = new DispatcherTimer();

        timer.Tick += Timer_Tick;
    }

    public void Start(
    IntPtr hwnd,
    int interval,
    bool useRandom,
    int minInterval,
    int maxInterval,
    bool enableAutoClick,
    int clickX,
    int clickY,
    int clickDelay)
    {
        currentWindow = hwnd;

        this.useRandom = useRandom;

        this.minInterval = minInterval;

        this.maxInterval = maxInterval;

        this.enableAutoClick = enableAutoClick;

        this.clickX = clickX;

        this.clickY = clickY;

        this.clickDelay = clickDelay;

        timer.Interval =
            TimeSpan.FromMilliseconds(interval);

        timer.Start();
        if (useRandom)
        {
            timer.Interval =
                TimeSpan.FromMilliseconds(
                    random.Next(
                        minInterval,
                        maxInterval + 1));
        }
    }

    public void Stop()
    {
        timer.Stop();
    }

    private void Timer_Tick(
        object? sender,
        EventArgs e)
    {
        if (currentWindow == IntPtr.Zero)
            return;

        if (enableAutoClick)
        {
            IntPtr lParam =
                (IntPtr)((clickY << 16) | (clickX & 0xFFFF));

            Win32.PostMessage(
                currentWindow,
                Win32.WM_LBUTTONDOWN,
                (IntPtr)1,
                lParam);

            Win32.PostMessage(
                currentWindow,
                Win32.WM_LBUTTONUP,
                IntPtr.Zero,
                lParam);

            Thread.Sleep(clickDelay);
        }

        Win32.PostMessage(
            currentWindow,
            Win32.WM_KEYDOWN,
            (IntPtr)Win32.VK_NEXT,
            IntPtr.Zero);

        Win32.PostMessage(
            currentWindow,
            Win32.WM_KEYUP,
            (IntPtr)Win32.VK_NEXT,
            IntPtr.Zero);
    }
}