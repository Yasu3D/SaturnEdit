using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SaturnEdit.Controls;

public partial class WindowChrome : UserControl
{
    public WindowChrome()
    {
        InitializeComponent();
    }

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        ButtonMinimize.Click += ButtonMinimize_OnClick;
        ButtonMaximize.Click += ButtonMaximize_OnClick;
        ButtonClose.Click += ButtonClose_OnClick;
        AttachedToVisualTree += Control_OnAttachedToVisualTree;
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        ButtonMinimize.Click -= ButtonMinimize_OnClick;
        ButtonMaximize.Click -= ButtonMaximize_OnClick;
        ButtonClose.Click -= ButtonClose_OnClick;
        AttachedToVisualTree -= Control_OnAttachedToVisualTree;
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is not Window window) return;
        
        window.Resized += Window_OnSizeChanged;
    }

    public void ButtonMinimize_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window) return;
        window.WindowState = WindowState.Minimized;
    }
    
    public void ButtonMaximize_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window) return;
        window.WindowState = window.WindowState switch
        {
            WindowState.Maximized => WindowState.Normal,
            WindowState.Normal => WindowState.Maximized,
            _ => window.WindowState,
        };
    }
    
    public void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window window)
        {
            window.Close();
        }
    }
    
    public async void Window_OnSizeChanged(object? sender, WindowResizedEventArgs e)
    {
        try
        {
            if (VisualRoot is not Window window) return;
        
            // Hacky :3
            await Task.Delay(1);

            IconMaximize.IsVisible = window.WindowState == WindowState.Normal;
            IconRestore.IsVisible = window.WindowState == WindowState.Maximized;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Handlers
}
