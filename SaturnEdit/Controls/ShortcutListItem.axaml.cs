using Avalonia.Controls;
using SaturnEdit.Settings;

namespace SaturnEdit.Controls;

public partial class ShortcutListItem : UserControl
{
    public ShortcutListItem()
    {
        InitializeComponent();
    }

    public string Key { get; set; } = "";

    public Shortcut Shortcut { get; set; } = new(Avalonia.Input.Key.None, false, false, false, "", "");
}