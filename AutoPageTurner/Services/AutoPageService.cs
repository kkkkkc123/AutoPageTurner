using AutoPageTurner.Helpers;
using AutoPageTurner.Models;

namespace AutoPageTurner.Services;

public sealed class AutoPageService : IDisposable
{
    private readonly object syncRoot = new();

    private CancellationTokenSource? cancellation;

    private Task? runningTask;

    public bool IsRunning
    {
        get
        {
            lock (syncRoot)
            {
                return runningTask is
                {
                    IsCompleted: false
                };
            }
        }
    }

    public event Action<int, int>? CountdownChanged;

    public event Action<string>? Stopped;

    public async Task StartAsync(
        AutoPageOptions options)
    {
        await StopAsync();

        CancellationTokenSource source =
            new();

        lock (syncRoot)
        {
            cancellation = source;
            runningTask =
                RunAsync(
                    options,
                    source.Token);
        }
    }

    public async Task StopAsync()
    {
        CancellationTokenSource? source;
        Task? task;

        lock (syncRoot)
        {
            source = cancellation;
            task = runningTask;
            cancellation = null;
            runningTask = null;
        }

        if (source == null)
        {
            return;
        }

        source.Cancel();

        if (task != null)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
        }

        source.Dispose();
    }

    public async Task<bool> TestPageAsync(
        AutoPageOptions options)
    {
        if (!Win32.IsWindow(
                options.WindowHandle))
        {
            return false;
        }

        Win32.POINT screenPoint =
            GetPageActionPoint(
                options);

        SendPageAction(
            options,
            GetMessageTarget(
                options.WindowHandle,
                screenPoint),
            screenPoint);

        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> TestClickAsync(
        AutoPageOptions options)
    {
        if (!Win32.IsWindow(
                options.WindowHandle))
        {
            return false;
        }

        await SendClickAsync(
            options,
            CancellationToken.None);

        return true;
    }

    public void Dispose()
    {
        CancellationTokenSource? source;

        lock (syncRoot)
        {
            source = cancellation;
            cancellation = null;
            runningTask = null;
        }

        source?.Cancel();
        source?.Dispose();
    }

    private async Task RunAsync(
        AutoPageOptions options,
        CancellationToken cancellationToken)
    {
        string stopReason =
            "已停止";

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!Win32.IsWindow(
                        options.WindowHandle))
                {
                    stopReason =
                        "目标窗口已关闭，任务自动停止";
                    break;
                }

                int interval =
                    GetNextInterval(
                        options);

                await WaitWithCountdownAsync(
                    interval,
                    cancellationToken);

                if (!Win32.IsWindow(
                        options.WindowHandle))
                {
                    stopReason =
                        "目标窗口已关闭，任务自动停止";
                    break;
                }

                if (options.EnableAutoClick)
                {
                    await SendClickAsync(
                        options,
                        cancellationToken);
                }

                Win32.POINT screenPoint =
                    GetPageActionPoint(
                        options);

                SendPageAction(
                    options,
                    GetMessageTarget(
                        options.WindowHandle,
                        screenPoint),
                    screenPoint);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (syncRoot)
            {
                if (cancellation?.Token ==
                    cancellationToken)
                {
                    cancellation.Dispose();
                    cancellation = null;
                    runningTask = null;
                }
            }

            Stopped?.Invoke(
                stopReason);
        }
    }

    private async Task WaitWithCountdownAsync(
        int interval,
        CancellationToken cancellationToken)
    {
        int remaining =
            interval;

        while (remaining > 0)
        {
            CountdownChanged?.Invoke(
                remaining,
                interval);

            int delay =
                Math.Min(
                    remaining,
                    1000);

            await Task.Delay(
                    delay,
                    cancellationToken)
                .ConfigureAwait(false);

            remaining -= delay;
        }

        CountdownChanged?.Invoke(
            0,
            interval);
    }

    private static int GetNextInterval(
        AutoPageOptions options)
    {
        if (!options.UseRandomInterval)
        {
            return options.Interval;
        }

        return Random.Shared.Next(
            options.MinInterval,
            options.MaxInterval + 1);
    }

    private static async Task SendClickAsync(
        AutoPageOptions options,
        CancellationToken cancellationToken)
    {
        Win32.POINT clickPoint =
            GetClickPoint(
                options);

        IntPtr messageTarget =
            GetMessageTarget(
                options.WindowHandle,
                clickPoint);

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

        if (options.ClickDelay > 0)
        {
            await Task.Delay(
                    options.ClickDelay,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static void SendPageAction(
        AutoPageOptions options,
        IntPtr messageTarget,
        Win32.POINT screenPoint)
    {
        if (options.PageAction == "鼠标滚轮")
        {
            Win32.PostMessage(
                messageTarget,
                Win32.WM_MOUSEWHEEL,
                MakeWheelWParam(-120),
                MakeLParam(
                    screenPoint.X,
                    screenPoint.Y));

            return;
        }

        int virtualKey =
            options.PageAction switch
            {
                "方向键下" => Win32.VK_DOWN,
                "空格键" => Win32.VK_SPACE,
                _ => Win32.VK_NEXT
            };

        Win32.PostMessage(
            messageTarget,
            Win32.WM_KEYDOWN,
            (IntPtr)virtualKey,
            IntPtr.Zero);

        Win32.PostMessage(
            messageTarget,
            Win32.WM_KEYUP,
            (IntPtr)virtualKey,
            IntPtr.Zero);
    }

    private static IntPtr GetMessageTarget(
        IntPtr window,
        Win32.POINT screenPoint)
    {
        IntPtr target =
            window;

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

    private static Win32.POINT GetClickPoint(
        AutoPageOptions options)
    {
        if (!options.EnableClickDrift ||
            options.ClickDriftRadius == 0)
        {
            return new Win32.POINT
            {
                X = options.ClickX,
                Y = options.ClickY
            };
        }

        double angle =
            Random.Shared.NextDouble() *
            Math.PI *
            2;

        double distance =
            Math.Sqrt(
                Random.Shared.NextDouble()) *
            options.ClickDriftRadius;

        return new Win32.POINT
        {
            X = options.ClickX +
                (int)Math.Round(
                    Math.Cos(angle) *
                    distance),
            Y = options.ClickY +
                (int)Math.Round(
                    Math.Sin(angle) *
                    distance)
        };
    }

    private static Win32.POINT GetWindowCenterPoint(
        IntPtr window)
    {
        if (Win32.GetWindowRect(
                window,
                out var rect))
        {
            return new Win32.POINT
            {
                X = rect.Left +
                    (rect.Right - rect.Left) / 2,
                Y = rect.Top +
                    (rect.Bottom - rect.Top) / 2
            };
        }

        return new Win32.POINT();
    }

    private static Win32.POINT GetPageActionPoint(
        AutoPageOptions options)
    {
        if (options.PageAction ==
            "鼠标滚轮")
        {
            return GetRandomizedPoint(
                options.WheelX,
                options.WheelY,
                options.EnableWheelDrift,
                options.WheelDriftRadius);
        }

        return GetWindowCenterPoint(
            options.WindowHandle);
    }

    private static Win32.POINT GetRandomizedPoint(
        int x,
        int y,
        bool enableDrift,
        int driftRadius)
    {
        if (!enableDrift ||
            driftRadius == 0)
        {
            return new Win32.POINT
            {
                X = x,
                Y = y
            };
        }

        double angle =
            Random.Shared.NextDouble() *
            Math.PI *
            2;

        double distance =
            Math.Sqrt(
                Random.Shared.NextDouble()) *
            driftRadius;

        return new Win32.POINT
        {
            X = x +
                (int)Math.Round(
                    Math.Cos(angle) *
                    distance),
            Y = y +
                (int)Math.Round(
                    Math.Sin(angle) *
                    distance)
        };
    }

    private static IntPtr MakeLParam(
        int low,
        int high)
    {
        uint value =
            (uint)(ushort)low |
            ((uint)(ushort)high << 16);

        return (IntPtr)(nint)value;
    }

    private static IntPtr MakeWheelWParam(
        short delta)
    {
        uint value =
            (uint)(ushort)delta << 16;

        return (IntPtr)(nint)value;
    }
}
