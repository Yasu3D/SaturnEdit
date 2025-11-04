using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using SaturnEdit.Systems;

namespace SaturnEdit.Controls;

public partial class SearchForAnythingListItem : UserControl
{
    public SearchForAnythingListItem()
    {
        InitializeComponent();
    }

    public string Key { get; private set; }

#region Methods
    public void SetData(string key, Shortcut shortcut)
    {
        Key = key;
        
        TextBlockGroup.Bind(TextBlock.TextProperty, new DynamicResourceExtension(shortcut.GroupMessage));
        TextBlockAction.Bind(TextBlock.TextProperty, new DynamicResourceExtension(shortcut.ActionMessage));
        TextBlockShortcut.Text = shortcut.ToString();
    }
#endregion Methods
}