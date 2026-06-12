using AutoPageTurner.Commands;
using AutoPageTurner.Helpers;
using AutoPageTurner.Models;
using AutoPageTurner.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AutoPageTurner.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly WindowService windowService = new();

    private readonly AutoPageService pageService = new();

    private readonly ConfigService configService = new();

    public ObservableCollection<WindowItem> Windows { get; }
        = new();

    private int interval = 2000;

    public int Interval
    {
        get => interval;
        set
        {
            interval = value;
            OnPropertyChanged();
        }
    }

    private bool useRandomInterval = true;

    public bool UseRandomInterval
    {
        get => useRandomInterval;
        set
        {
            useRandomInterval = value;
            OnPropertyChanged();
        }
    }

    private int minInterval = 1500;

    public int MinInterval
    {
        get => minInterval;
        set
        {
            minInterval = value;
            OnPropertyChanged();
        }
    }

    private int maxInterval = 3500;

    public int MaxInterval
    {
        get => maxInterval;
        set
        {
            maxInterval = value;
            OnPropertyChanged();
        }
    }

    private bool enableAutoClick;

    public bool EnableAutoClick
    {
        get => enableAutoClick;
        set
        {
            enableAutoClick = value;
            OnPropertyChanged();
        }
    }

    private int clickX = 1000;

    public int ClickX
    {
        get => clickX;
        set
        {
            clickX = value;
            OnPropertyChanged();
        }
    }

    private int clickY = 500;

    public int ClickY
    {
        get => clickY;
        set
        {
            clickY = value;
            OnPropertyChanged();
        }
    }

    private int clickDelay = 100;

    public int ClickDelay
    {
        get => clickDelay;
        set
        {
            clickDelay = value;
            OnPropertyChanged();
        }
    }

    private bool enableClickDrift;

    public bool EnableClickDrift
    {
        get => enableClickDrift;
        set
        {
            enableClickDrift = value;
            OnPropertyChanged();
        }
    }

    private int clickDriftRadius = 10;

    public int ClickDriftRadius
    {
        get => clickDriftRadius;
        set
        {
            clickDriftRadius = value;
            OnPropertyChanged();
        }
    }

    private bool isPickingPoint;

    public ICommand RefreshCommand { get; }

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

    public ICommand PickPointCommand { get; }

    private string status = "未运行";

    public string Status
    {
        get => status;
        set
        {
            status = value;
            OnPropertyChanged();
        }
    }

    private WindowItem? selectedWindow;

    public WindowItem? SelectedWindow
    {
        get => selectedWindow;
        set
        {
            selectedWindow = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        LoadConfiguration();

        RefreshCommand =
            new RelayCommand(LoadWindows);

        StartCommand =
            new RelayCommand(Start);

        StopCommand =
            new RelayCommand(Stop);

        PickPointCommand =
            new RelayCommand(PickPoint);

        LoadWindows();
    }

    public void LoadWindows()
    {
        Windows.Clear();

        foreach (var item in windowService.GetWindows())
        {
            Windows.Add(item);
        }
    }

    private void Start()
    {
        SaveConfiguration();

        if (SelectedWindow == null)
        {
            MessageBox.Show("请选择窗口");

            return;
        }

        pageService.Start(
            SelectedWindow.Handle,
            Interval,
            UseRandomInterval,
            MinInterval,
            MaxInterval,
            EnableAutoClick,
            ClickX,
            ClickY,
            ClickDelay,
            EnableClickDrift,
            ClickDriftRadius);

        Status = "运行中";
    }

    private void Stop()
    {
        pageService.Stop();

        Status = "已停止";
    }

    public void Shutdown()
    {
        pageService.Stop();

        SaveConfiguration();
    }

    private void LoadConfiguration()
    {
        AppConfig config =
            configService.Load();

        Interval = config.Interval;
        UseRandomInterval = config.UseRandomInterval;
        MinInterval = config.MinInterval;
        MaxInterval = config.MaxInterval;
        EnableAutoClick = config.EnableAutoClick;
        ClickX = config.ClickX;
        ClickY = config.ClickY;
        ClickDelay = config.ClickDelay;
        EnableClickDrift = config.EnableClickDrift;
        ClickDriftRadius = config.ClickDriftRadius;
    }

    private void SaveConfiguration()
    {
        configService.Save(
            new AppConfig
            {
                Interval = Interval,
                UseRandomInterval = UseRandomInterval,
                MinInterval = MinInterval,
                MaxInterval = MaxInterval,
                EnableAutoClick = EnableAutoClick,
                ClickX = ClickX,
                ClickY = ClickY,
                ClickDelay = ClickDelay,
                EnableClickDrift = EnableClickDrift,
                ClickDriftRadius = ClickDriftRadius
            });
    }

    private async void PickPoint()
    {
        if (isPickingPoint)
        {
            return;
        }

        isPickingPoint = true;

        try
        {
            for (int seconds = 3;
                 seconds > 0;
                 seconds--)
            {
                Status =
                    $"{seconds}秒后获取坐标";

                await Task.Delay(1000);
            }

            Status = "正在获取坐标";

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
        }
    }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
