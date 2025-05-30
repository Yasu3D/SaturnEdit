using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SaturnEdit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        WindowChrome.ButtonMinimize.Click += ButtonMinimize_OnClick;
        WindowChrome.ButtonMaximize.Click += ButtonMaximize_OnClick;
        WindowChrome.ButtonClose.Click += ButtonClose_OnClick;
        Resized += Window_OnSizeChanged;
    }

    public void ButtonMinimize_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    
    public void ButtonMaximize_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState switch
        {
            WindowState.Maximized => WindowState.Normal,
            WindowState.Normal => WindowState.Maximized,
            _ => WindowState,
        };
    }
    
    public void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    public async void Window_OnSizeChanged(object? sender, WindowResizedEventArgs e)
    {
        // Hacky :3
        await Task.Delay(1);

        WindowChrome.IconMaximize.IsVisible = WindowState == WindowState.Normal;
        WindowChrome.IconRestore.IsVisible = WindowState == WindowState.Maximized;
    }
}