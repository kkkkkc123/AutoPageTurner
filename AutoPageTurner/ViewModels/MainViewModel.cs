using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using AutoPageTurner.Commands;
using AutoPageTurner.Helpers;
using AutoPageTurner.Models;
using AutoPageTurner.Services;

namespace AutoPageTurner.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const int MinimumInterval = 100;

    private const int MaximumInterval = 86_400_000;

    private readonly WindowService windowService =
        new();

    private readonly AutoPageService pageService =
        new();

    private readonly ConfigService configService =
        new();

    private string savedWindowTitle =
        "";

    private string savedProcessName =
        "";

    private int interval = 2000;

    private bool useRandomInterval = true;

    private int minInterval = 1500;

    private int maxInterval = 3500;

    private bool enableAutoClick;

    private int clickX = 1000;

    private int clickY = 500;

    private int clickDelay = 100;

    private bool enableClickDrift;

    private int clickDriftRadius = 10;

    private string pageAction =
        "PageDown";

    private string status =
        "未运行";

    private string countdownText =
        "等待开始";

    private bool isRunning;

    private bool isPickingPoint;

    private WindowItem? selectedWindow;

    public MainViewModel()
    {
        pageService.CountdownChanged +=
            OnCountdownChanged;

        pageService.Stopped +=
            OnServiceStopped;

        LoadConfiguration();

        RefreshCommand =
            new RelayCommand(
                LoadWindows,
                () => !IsRunning);

        StartCommand =
            new RelayCommand(
                Start,
                () => !IsRunning &&
                      SelectedWindow != null);

        StopCommand =
            new RelayCommand(
                Stop,
                () => IsRunning);

        TestPageCommand =
            new RelayCommand(
                TestPage,
                () => !IsRunning &&
                      SelectedWindow != null);

        TestClickCommand =
            new RelayCommand(
                TestClick,
                () => !IsRunning &&
                      SelectedWindow != null &&
                      EnableAutoClick);

        PickPointCommand =
            new RelayCommand(
                PickPoint,
                () => !IsRunning &&
                      !isPickingPoint);

        ResetCommand =
            new RelayCommand(
                ResetConfiguration,
                () => !IsRunning);

        LoadWindows();
    }

    public ObservableCollection<WindowItem> Windows { get; } =
        [];

    public IReadOnlyList<string> PageActions { get; } =
        [
            "PageDown",
            "方向键下",
            "空格键",
            "鼠标滚轮"
        ];

    public RelayCommand RefreshCommand { get; }

    public RelayCommand StartCommand { get; }

    public RelayCommand StopCommand { get; }

    public RelayCommand TestPageCommand { get; }

    public RelayCommand TestClickCommand { get; }

    public RelayCommand PickPointCommand { get; }

    public RelayCommand ResetCommand { get; }

    public string ConfigFilePath =>
        configService.FilePath;

    public int Interval
    {
        get => interval;
        set => SetField(
            ref interval,
            value);
    }

    public bool UseRandomInterval
    {
        get => useRandomInterval;
        set
        {
            if (SetField(
                    ref useRandomInterval,
                    value))
            {
                NotifyEditingState();
            }
        }
    }

    public int MinInterval
    {
        get => minInterval;
        set => SetField(
            ref minInterval,
            value);
    }

    public int MaxInterval
    {
        get => maxInterval;
        set => SetField(
            ref maxInterval,
            value);
    }

    public bool EnableAutoClick
    {
        get => enableAutoClick;
        set
        {
            if (SetField(
                    ref enableAutoClick,
                    value))
            {
                NotifyEditingState();
                RaiseCommandStates();
            }
        }
    }

    public int ClickX
    {
        get => clickX;
        set => SetField(
            ref clickX,
            value);
    }

    public int ClickY
    {
        get => clickY;
        set => SetField(
            ref clickY,
            value);
    }

    public int ClickDelay
    {
        get => clickDelay;
        set => SetField(
            ref clickDelay,
            value);
    }

    public bool EnableClickDrift
    {
        get => enableClickDrift;
        set
        {
            if (SetField(
                    ref enableClickDrift,
                    value))
            {
                OnPropertyChanged(
                    nameof(IsDriftSettingsEnabled));
            }
        }
    }

    public int ClickDriftRadius
    {
        get => clickDriftRadius;
        set => SetField(
            ref clickDriftRadius,
            value);
    }

    public string PageAction
    {
        get => pageAction;
        set => SetField(
            ref pageAction,
            value);
    }

    public string Status
    {
        get => status;
        private set => SetField(
            ref status,
            value);
    }

    public string CountdownText
    {
        get => countdownText;
        private set => SetField(
            ref countdownText,
            value);
    }

    public bool IsRunning
    {
        get => isRunning;
        private set
        {
            if (SetField(
                    ref isRunning,
                    value))
            {
                NotifyEditingState();
                RaiseCommandStates();
            }
        }
    }

    public bool IsEditingEnabled =>
        !IsRunning;

    public bool IsFixedIntervalEnabled =>
        !IsRunning &&
        !UseRandomInterval;

    public bool IsRandomIntervalEnabled =>
        !IsRunning &&
        UseRandomInterval;

    public bool IsClickSettingsEnabled =>
        !IsRunning &&
        EnableAutoClick;

    public bool IsDriftSettingsEnabled =>
        IsClickSettingsEnabled &&
        EnableClickDrift;

    public WindowItem? SelectedWindow
    {
        get => selectedWindow;
        set
        {
            if (SetField(
                    ref selectedWindow,
                    value))
            {
                RaiseCommandStates();
            }
        }
    }

    public void LoadWindows()
    {
        string currentTitle =
            SelectedWindow?.Title ??
            savedWindowTitle;

        string currentProcess =
            SelectedWindow?.ProcessName ??
            savedProcessName;

        Windows.Clear();

        foreach (WindowItem item in
                 windowService.GetWindows())
        {
            Windows.Add(
                item);
        }

        SelectedWindow =
            Windows.FirstOrDefault(
                item =>
                    !string.IsNullOrWhiteSpace(
                        currentProcess) &&
                    item.ProcessName.Equals(
                        currentProcess,
                        StringComparison.OrdinalIgnoreCase) &&
                    item.Title.Equals(
                        currentTitle,
                        StringComparison.OrdinalIgnoreCase))
            ?? Windows.FirstOrDefault(
                item =>
                    !string.IsNullOrWhiteSpace(
                        currentProcess) &&
                    item.ProcessName.Equals(
                        currentProcess,
                        StringComparison.OrdinalIgnoreCase));

        Status =
            SelectedWindow == null &&
            !string.IsNullOrWhiteSpace(
                currentProcess)
                ? "未找到上次使用的目标窗口"
                : Status;
    }

    public void Shutdown()
    {
        pageService.CountdownChanged -=
            OnCountdownChanged;

        pageService.Stopped -=
            OnServiceStopped;

        pageService.Dispose();
        SaveConfiguration(
            false);
    }

    private async void Start()
    {
        if (!TryCreateOptions(
                out AutoPageOptions options,
                true))
        {
            return;
        }

        if (!SaveConfiguration(
                true))
        {
            return;
        }

        IsRunning = true;
        Status = "运行中";

        LogService.Write(
            $"启动任务：{SelectedWindow?.Title}");

        await pageService.StartAsync(
            options);
    }

    private async void Stop()
    {
        await pageService.StopAsync();

        IsRunning = false;
        Status = "已停止";
        CountdownText = "等待开始";

        LogService.Write(
            "用户停止任务");
    }

    private async void TestPage()
    {
        if (!TryCreateOptions(
                out AutoPageOptions options,
                false))
        {
            return;
        }

        bool succeeded =
            await pageService.TestPageAsync(
                options);

        Status =
            succeeded
                ? "已发送测试翻页"
                : "目标窗口已失效";
    }

    private async void TestClick()
    {
        if (!TryCreateOptions(
                out AutoPageOptions options,
                false))
        {
            return;
        }

        bool succeeded =
            await pageService.TestClickAsync(
                options);

        Status =
            succeeded
                ? "已发送测试点击"
                : "目标窗口已失效";
    }

    private async void PickPoint()
    {
        if (isPickingPoint)
        {
            return;
        }

        isPickingPoint = true;
        RaiseCommandStates();

        try
        {
            for (int seconds = 3;
                 seconds > 0;
                 seconds--)
            {
                Status =
                    $"{seconds}秒后获取坐标";

                await Task.Delay(
                    1000);
            }

            Win32.GetCursorPos(
                out var point);

            ClickX = point.X;
            ClickY = point.Y;

            Status =
                $"坐标: {point.X},{point.Y}";
        }
        finally
        {
            isPickingPoint = false;
            RaiseCommandStates();
        }
    }

    private void ResetConfiguration()
    {
        MessageBoxResult result =
            MessageBox.Show(
                "确定恢复默认设置吗？",
                "恢复默认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

        if (result !=
            MessageBoxResult.Yes)
        {
            return;
        }

        if (!configService.TryReset(
                out string? error))
        {
            MessageBox.Show(
                $"无法删除配置文件：{error}",
                "恢复失败",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        ApplyConfiguration(
            new AppConfig());

        SelectedWindow = null;
        Status = "已恢复默认设置";
        CountdownText = "等待开始";
    }

    private bool TryCreateOptions(
        out AutoPageOptions options,
        bool requireInterval)
    {
        options = null!;

        if (SelectedWindow == null)
        {
            ShowValidationError(
                "请选择目标窗口。");
            return false;
        }

        if (!Win32.IsWindow(
                SelectedWindow.Handle))
        {
            ShowValidationError(
                "目标窗口已关闭，请刷新窗口列表。");
            return false;
        }

        if (requireInterval)
        {
            if (!UseRandomInterval &&
                !IsValidInterval(
                    Interval))
            {
                ShowValidationError(
                    $"固定间隔必须在 {MinimumInterval} 到 {MaximumInterval} 毫秒之间。");
                return false;
            }

            if (UseRandomInterval &&
                (!IsValidInterval(
                     MinInterval) ||
                 !IsValidInterval(
                     MaxInterval) ||
                 MinInterval >
                 MaxInterval))
            {
                ShowValidationError(
                    "随机间隔范围无效，且最小间隔不能大于最大间隔。");
                return false;
            }
        }

        if (ClickDelay is < 0 or > 60_000)
        {
            ShowValidationError(
                "点击后等待必须在 0 到 60000 毫秒之间。");
            return false;
        }

        if (ClickDriftRadius is < 0 or > 1000)
        {
            ShowValidationError(
                "点击漂移半径必须在 0 到 1000 像素之间。");
            return false;
        }

        options =
            new AutoPageOptions
            {
                WindowHandle =
                    SelectedWindow.Handle,
                Interval =
                    Interval,
                UseRandomInterval =
                    UseRandomInterval,
                MinInterval =
                    MinInterval,
                MaxInterval =
                    MaxInterval,
                EnableAutoClick =
                    EnableAutoClick,
                ClickX =
                    ClickX,
                ClickY =
                    ClickY,
                ClickDelay =
                    ClickDelay,
                EnableClickDrift =
                    EnableClickDrift,
                ClickDriftRadius =
                    ClickDriftRadius,
                PageAction =
                    PageAction
            };

        return true;
    }

    private bool SaveConfiguration(
        bool showError)
    {
        AppConfig config =
            CreateConfiguration();

        bool saved =
            configService.TrySave(
                config,
                out string? error);

        if (!saved &&
            showError)
        {
            LogService.Write(
                $"保存配置失败：{error}");

            MessageBox.Show(
                $"无法保存配置：{error}",
                "保存失败",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        return saved;
    }

    private AppConfig CreateConfiguration()
    {
        return new AppConfig
        {
            Interval =
                Interval,
            UseRandomInterval =
                UseRandomInterval,
            MinInterval =
                MinInterval,
            MaxInterval =
                MaxInterval,
            EnableAutoClick =
                EnableAutoClick,
            ClickX =
                ClickX,
            ClickY =
                ClickY,
            ClickDelay =
                ClickDelay,
            EnableClickDrift =
                EnableClickDrift,
            ClickDriftRadius =
                ClickDriftRadius,
            PageAction =
                PageAction,
            TargetWindowTitle =
                SelectedWindow?.Title ?? "",
            TargetProcessName =
                SelectedWindow?.ProcessName ?? ""
        };
    }

    private void LoadConfiguration()
    {
        ApplyConfiguration(
            configService.Load());
    }

    private void ApplyConfiguration(
        AppConfig config)
    {
        Interval =
            NormalizeInterval(
                config.Interval,
                2000);

        UseRandomInterval =
            config.UseRandomInterval;

        MinInterval =
            NormalizeInterval(
                config.MinInterval,
                1500);

        MaxInterval =
            NormalizeInterval(
                config.MaxInterval,
                3500);

        if (MinInterval >
            MaxInterval)
        {
            (MinInterval, MaxInterval) =
                (MaxInterval, MinInterval);
        }

        EnableAutoClick =
            config.EnableAutoClick;

        ClickX =
            config.ClickX;

        ClickY =
            config.ClickY;

        ClickDelay =
            Math.Clamp(
                config.ClickDelay,
                0,
                60_000);

        EnableClickDrift =
            config.EnableClickDrift;

        ClickDriftRadius =
            Math.Clamp(
                config.ClickDriftRadius,
                0,
                1000);

        PageAction =
            PageActions.Contains(
                config.PageAction)
                ? config.PageAction
                : "PageDown";

        savedWindowTitle =
            config.TargetWindowTitle;

        savedProcessName =
            config.TargetProcessName;
    }

    private void OnCountdownChanged(
        int remainingMilliseconds,
        int intervalMilliseconds)
    {
        Application.Current.Dispatcher.BeginInvoke(
            () =>
            {
                double remainingSeconds =
                    remainingMilliseconds /
                    1000d;

                CountdownText =
                    remainingMilliseconds > 0
                        ? $"下次翻页：{remainingSeconds:0.0} 秒（本次间隔 {intervalMilliseconds} ms）"
                        : "正在翻页";
            });
    }

    private void OnServiceStopped(
        string reason)
    {
        Application.Current.Dispatcher.BeginInvoke(
            () =>
            {
                if (!IsRunning)
                {
                    return;
                }

                IsRunning = false;
                Status = reason;
                CountdownText = "等待开始";

                LogService.Write(
                    reason);
            });
    }

    private void NotifyEditingState()
    {
        OnPropertyChanged(
            nameof(IsEditingEnabled));
        OnPropertyChanged(
            nameof(IsFixedIntervalEnabled));
        OnPropertyChanged(
            nameof(IsRandomIntervalEnabled));
        OnPropertyChanged(
            nameof(IsClickSettingsEnabled));
        OnPropertyChanged(
            nameof(IsDriftSettingsEnabled));
    }

    private void RaiseCommandStates()
    {
        RefreshCommand?.RaiseCanExecuteChanged();
        StartCommand?.RaiseCanExecuteChanged();
        StopCommand?.RaiseCanExecuteChanged();
        TestPageCommand?.RaiseCanExecuteChanged();
        TestClickCommand?.RaiseCanExecuteChanged();
        PickPointCommand?.RaiseCanExecuteChanged();
        ResetCommand?.RaiseCanExecuteChanged();
    }

    private static bool IsValidInterval(
        int value)
    {
        return value is >= MinimumInterval and
               <= MaximumInterval;
    }

    private static int NormalizeInterval(
        int value,
        int fallback)
    {
        return IsValidInterval(
                   value)
            ? value
            : fallback;
    }

    private void ShowValidationError(
        string message)
    {
        Status =
            message;

        MessageBox.Show(
            message,
            "设置无效",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(
                field,
                value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(
            propertyName);
        return true;
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(
                propertyName));
    }
}
