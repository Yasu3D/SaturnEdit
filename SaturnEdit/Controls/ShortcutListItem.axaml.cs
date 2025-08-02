using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SaturnEdit.Systems;

namespace SaturnEdit.Controls;

public partial class ShortcutListItem : UserControl
{
    public ShortcutListItem()
    {
        InitializeComponent();
    }

    public string ActionName { get; set; } = "";
    
    public Shortcut Shortcut { get; set; }
}