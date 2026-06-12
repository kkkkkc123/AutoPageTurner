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
            ClickDelay);

        Status = "运行中";
    }

    private void Stop()
    {
        pageService.Stop();

        Status = "已停止";
    }

    private async void PickPoint()
    {
        Status = "3秒后开始获取坐标";

        await Task.Delay(3000);

        Win32.GetCursorPos(out var point);

        ClickX = point.X;

        ClickY = point.Y;

        Status = $"坐标: {point.X},{point.Y}";
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