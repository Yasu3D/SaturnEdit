using System;
using Avalonia.Controls;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class BookmarkListView : UserControl
{
    public BookmarkListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        TextBlockShortcutMoveItemUp.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
        TextBlockShortcutDeleteSelection.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
    }
}