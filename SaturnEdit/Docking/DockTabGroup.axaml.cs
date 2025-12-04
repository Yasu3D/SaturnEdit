using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SaturnEdit.Docking;

public partial class DockTabGroup : UserControl
{
    public DockTabGroup()
    {
        InitializeComponent();
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
    
#region System Event Handlers
    private void OnWindowDragStarted(object? sender, EventArgs e) => UpdateTarget();

    private void OnWindowDragEnded(object? sender, EventArgs e)
    {
        Target.IsVisible = false;
        
        if (DockArea.Instance == null) return;
        
        if (Target.TargetSide != TargetSide.None && DockArea.Instance.DraggedGroup != null)
        {
            DockArea.Instance.Dock(Target, DockArea.Instance.DraggedGroup);
        }

        Target.TargetSide = TargetSide.None;
    }

    private void OnWindowDragged(object? sender, EventArgs e) => UpdateTarget();
#endregion System Event Handlers
    
#region UI Event Handlers
    private void Visual_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DockArea.Instance != null)
        {
            DockArea.Instance.WindowDragStarted += OnWindowDragStarted;
            DockArea.Instance.WindowDragEnded += OnWindowDragEnded;
            DockArea.Instance.WindowDragged += OnWindowDragged;
        }
        
        ButtonClose.IsVisible = !IsFloating;
        WindowHandle.Margin = IsFloating ? new(0, 0, 12, 0) : new(0, 0, 0, 0);
    }
    
    private void Visual_OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (DockArea.Instance != null)
        {
            DockArea.Instance.WindowDragStarted -= OnWindowDragStarted;
            DockArea.Instance.WindowDragEnded -= OnWindowDragEnded;
            DockArea.Instance.WindowDragged -= OnWindowDragged;
        }
    }

    private void ListBoxTabs_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TabList?.SelectedItem is not DockTab tab) return;
        
        TabContentContainer.Content = tab.TabContent;
    }
    
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        DockArea.Instance?.RemoveTabGroup(this);
    }
    
    private void WindowHandle_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;
        
        DockArea.Instance.PointerPosition = this.PointToScreen(e.GetPosition(VisualRoot as Window));

        if (VisualRoot is not DockWindow dockWindow)
        {
            int x = DockArea.Instance.PointerPosition.X - DockArea.Instance.StartPosition.X;
            int y = DockArea.Instance.PointerPosition.Y - DockArea.Instance.StartPosition.Y;

            if (Math.Abs(x + y) > 20)
            {
                DockArea.Instance.Float(this);
            }
        }
        else
        {
            int x = (int)(DockArea.Instance.PointerPosition.X - DockArea.Instance.WindowOffset.X);
            int y = (int)(DockArea.Instance.PointerPosition.Y - DockArea.Instance.WindowOffset.Y);

            dockWindow.Position = new(x, y);
        }
        
        UpdateTarget();
    }
    
    private void WindowHandle_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;
        
        DockArea.Instance.StartPosition = this.PointToScreen(e.GetPosition(VisualRoot as Window));
        DockArea.Instance.PointerPosition = DockArea.Instance.StartPosition;
        
        DockArea.Instance.WindowOffset = e.GetPosition(VisualRoot as Window);
        DockArea.Instance.DraggedGroup = this;
        
        if (VisualRoot is DockWindow window)
        {
            DockArea.Instance.WindowDragActive = true;

            dragActive = true;
            window.Opacity = 0.5;
        }
        
        UpdateTarget();
    }

    private void WindowHandle_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        
        DockArea.Instance.WindowDragActive = false;
        dragActive = false;
        
        if (VisualRoot is DockWindow window)
        {
            window.Opacity = 1.0;
        }
    }
#endregion UI Event Handlers
}