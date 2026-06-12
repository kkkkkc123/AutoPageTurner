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

    private bool enableClickDrift;

    private int clickDriftRadius;

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
    int clickDelay,
    bool enableClickDrift,
    int clickDriftRadius)
    {
        currentWindow = hwnd;

        this.useRandom = useRandom;

        this.minInterval = minInterval;

        this.maxInterval = maxInterval;

        this.enableAutoClick = enableAutoClick;

        this.clickX = clickX;

        this.clickY = clickY;

        this.clickDelay = clickDelay;

        this.enableClickDrift = enableClickDrift;

        this.clickDriftRadius = clickDriftRadius;

        if (useRandom)
        {
            SetRandomInterval();
        }
        else
        {
            timer.Interval =
                TimeSpan.FromMilliseconds(interval);
        }

        timer.Start();
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

        Win32.POINT targetPoint =
            enableAutoClick
                ? GetClickPoint()
                : GetWindowCenterPoint();

        IntPtr messageTarget =
            GetMessageTarget(
                targetPoint);

        if (enableAutoClick)
        {
            Win32.POINT clickPoint =
                targetPoint;

            Win32.ScreenToClient(
                messageTarget,
                ref clickPoint);

            IntPtr lParam =
                MakeLParam(
                    clickPoint.X,
                    clickPoint.Y);

            Win32.PostMessage(
                messageTarget,
                Win32.WM_LBUTTONDOWN,
                (IntPtr)1,
                lParam);

            Win32.PostMessage(
                messageTarget,
                Win32.WM_LBUTTONUP,
                IntPtr.Zero,
                lParam);

            Thread.Sleep(clickDelay);
        }

        Win32.PostMessage(
            messageTarget,
            Win32.WM_KEYDOWN,
            (IntPtr)Win32.VK_NEXT,
            IntPtr.Zero);

        Win32.PostMessage(
            messageTarget,
            Win32.WM_KEYUP,
            (IntPtr)Win32.VK_NEXT,
            IntPtr.Zero);

        if (useRandom)
        {
            SetRandomInterval();
        }
    }

    private void SetRandomInterval()
    {
        int min =
            Math.Min(
                minInterval,
                maxInterval);

        int max =
            Math.Max(
                minInterval,
                maxInterval);

        timer.Interval =
            TimeSpan.FromMilliseconds(
                random.Next(
                    min,
                max + 1));
    }

    private IntPtr GetMessageTarget(
        Win32.POINT screenPoint)
    {
        IntPtr target =
            currentWindow;

        while (true)
        {
            Win32.POINT clientPoint =
                screenPoint;

            if (!Win32.ScreenToClient(
                    target,
                    ref clientPoint))
            {
                return target;
            }

            IntPtr child =
                Win32.ChildWindowFromPointEx(
                    target,
                    clientPoint,
                    Win32.CWP_SKIPINVISIBLE);

            if (child == IntPtr.Zero ||
                child == target)
            {
                return target;
            }

            target = child;
        }
    }

    private Win32.POINT GetClickPoint()
    {
        if (!enableClickDrift ||
            clickDriftRadius == 0)
        {
            return new Win32.POINT
            {
                X = clickX,
                Y = clickY
            };
        }

        double radius =
            Math.Min(
                Math.Abs(
                    (long)clickDriftRadius),
                int.MaxValue);

        double angle =
            random.NextDouble() *
            Math.PI *
            2;

        double distance =
            Math.Sqrt(
                random.NextDouble()) *
            radius;

        long offsetX =
            (long)Math.Round(
                Math.Cos(angle) *
                distance);

        long offsetY =
            (long)Math.Round(
                Math.Sin(angle) *
                distance);

        return new Win32.POINT
        {
            X = (int)Math.Clamp(
                clickX + offsetX,
                int.MinValue,
                int.MaxValue),
            Y = (int)Math.Clamp(
                clickY + offsetY,
                int.MinValue,
                int.MaxValue)
        };
    }

    private Win32.POINT GetWindowCenterPoint()
    {
        if (Win32.GetWindowRect(
                currentWindow,
                out var rect))
        {
            return new Win32.POINT
            {
                X = rect.Left + (rect.Right - rect.Left) / 2,
                Y = rect.Top + (rect.Bottom - rect.Top) / 2
            };
        }

        return new Win32.POINT();
    }

    private static IntPtr MakeLParam(
        int low,
        int high)
    {
        return (IntPtr)((high << 16) | (low & 0xFFFF));
    }
}
