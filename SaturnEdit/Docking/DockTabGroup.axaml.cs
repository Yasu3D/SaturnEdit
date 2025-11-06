using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace SaturnEdit.Docking;

public partial class DockTabGroup : UserControl
{
    public DockTabGroup()
    {
        InitializeComponent();
        
        if (DockArea.Instance != null)
        {
            DockArea.Instance.WindowDragStarted += OnWindowDragStarted;
            DockArea.Instance.WindowDragEnded += OnWindowDragEnded;
            DockArea.Instance.WindowDragged += OnWindowDragged;
        }
    }
    
    public DockTab? SelectedTab
    {
        get
        {
            if (TabList.Items.Count == 0) return null;
            if (TabList.SelectedItem is not DockTab tab) return null;

            return tab;
        }
        set
        {
            if (value == null || !TabList.Items.Contains(value))
            {
                TabList.SelectedItem = null;
                return;
            }

            TabList.SelectedItem = value;
        }
    }
    
    public bool IsFloating => VisualRoot is DockWindow;
    private bool dragActive = false;

#region Methods
    private void UpdateTarget()
    {
        if (DockArea.Instance == null) return;
        if (VisualRoot == null) return;
        
        if (!DockArea.Instance.WindowDragActive || dragActive)
        {
            Target.IsVisible = false;
            return;
        }

        Target.ShowSideTargets = VisualRoot is not DockWindow;
        
        Rect bounds = DockArea.ScreenBounds(this);
        Target.IsVisible = DockArea.Instance.HitTest(bounds);
    }
#endregion Methods
    
#region System Event Delegates
    private void OnWindowDragStarted(object? sender, EventArgs e) => UpdateTarget();

    private void OnWindowDragEnded(object? sender, EventArgs e) => Target.IsVisible = false;

    private void OnWindowDragged(object? sender, EventArgs e) => UpdateTarget();
#endregion System Event Delegates
    
#region UI Event Delegates
    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        ButtonClose.IsVisible = !IsFloating;
    }

    private void ListBoxTabs_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TabList?.SelectedItem is not DockTab tab) return;
        
        TabContentContainer.Content = tab.TabContent;
    }
    
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        DockArea.Instance?.FloatTabGroup(this);
        //DockArea.Instance?.RemoveTabGroup(this);
    }
    
    private void WindowHandle_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;
        if (VisualRoot is not DockWindow window) return;
        
        DockArea.Instance.PointerPosition = this.PointToScreen(e.GetPosition(window));
        
        int x = (int)(DockArea.Instance.PointerPosition.X - DockArea.Instance.WindowOffset.X);
        int y = (int)(DockArea.Instance.PointerPosition.Y - DockArea.Instance.WindowOffset.Y);

        window.Position = new(x, y);
    }
    
    private void WindowHandle_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;
        if (VisualRoot is not DockWindow window) return;
        
        DockArea.Instance.WindowOffset = e.GetPosition(window);
        DockArea.Instance.WindowDragActive = true;
        
        dragActive = true;
        window.Opacity = 0.5;
    }

    private void WindowHandle_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        if (VisualRoot is not DockWindow window) return;
        
        DockArea.Instance.WindowDragActive = false;
        
        dragActive = false;
        window.Opacity = 1.0;
    }
#endregion UI Event Delegates
}