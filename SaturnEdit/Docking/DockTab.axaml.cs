using Avalonia.Controls;
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

#region UI Event Delegates
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Parent?.Parent?.Parent is not DockTabGroup group) return;

        DockArea.Instance?.RemoveTab(group, this);
    }
#endregion UI Event Delegates
}