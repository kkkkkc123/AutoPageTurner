using System.Windows;
using AutoPageTurner.ViewModels;

namespace AutoPageTurner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel();
    }
}