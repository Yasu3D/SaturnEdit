using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Common;

namespace SaturnEdit.Docking;

public partial class DockTab : UserControl
{
    public DockTab()
    {
        InitializeComponent();
    }
    
    public DockTab(UserControl tabContent, Icon icon, string titleKey)
    {
        InitializeComponent();
        
        TabContent = tabContent;

        Icon.Icon = icon;
        TextBlockTitle.Bind(TextBlock.TextProperty, new DynamicResourceExtension(titleKey));
    }
    
    public UserControl? TabContent { get; set; } = null;
    private Point? startPoint = null;

#region UI Event Delegates
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;

        startPoint = e.GetPosition(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (e.InitialPressMouseButton != MouseButton.Left) return;

        startPoint = null;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (!e.Properties.IsLeftButtonPressed) return;
        if (startPoint == null) return;
        
        Point p = e.GetPosition(this);
        double x = Math.Abs(startPoint.Value.X - p.X);
        double y = Math.Abs(startPoint.Value.Y - p.Y);
        
        if (x + y > 20)
        {
            DockArea.Instance.Float(this);
        }
    }
    
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DockArea.Instance == null) return;
        if (Parent?.Parent?.Parent is not DockTabGroup group) return;

        DockArea.Instance.RemoveTab(group, this);
    }
#endregion UI Event Delegates
}