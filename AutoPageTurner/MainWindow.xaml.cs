using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using AutoPageTurner.ViewModels;

namespace AutoPageTurner;

public partial class MainWindow : Window
{
    private readonly MainViewModel viewModel;

    public MainWindow()
    {
        InitializeComponent();

        SetWindowIcon();

        viewModel =
            new MainViewModel();

        DataContext = viewModel;
    }

    protected override void OnClosed(
        EventArgs e)
    {
        viewModel.Shutdown();

        base.OnClosed(e);
    }

    private void SetWindowIcon()
    {
        using var icon =
            System.Drawing.Icon.ExtractAssociatedIcon(
                Environment.ProcessPath ?? string.Empty);

        if (icon == null)
        {
            return;
        }

        Icon =
            Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
    }
}
