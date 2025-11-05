using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FluentIcons.Common;

namespace SaturnEdit.Docking;

public partial class DockTabGroupListItem : UserControl
{
    public DockTabGroupListItem()
    {
        InitializeComponent();
    }

#region Methods
    public void SetItem(Icon icon, string key)
    {
        Icon.Icon = icon;
        TextBlockTitle.Bind(TextBlock.TextProperty, new DynamicResourceExtension(key));
    }
#endregion Methods
}