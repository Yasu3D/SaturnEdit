using System;
using Avalonia.Controls;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class LayerListView : UserControl
{
    public LayerListView()
        {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        TextBlockShortcutMoveItemUp1.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown1.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
        TextBlockShortcutDeleteSelection1.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        
        TextBlockShortcutMoveItemUp2.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown2.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
        TextBlockShortcutDeleteSelection2.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        
        TextBlockShortcutMoveItemUp3.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown3.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
    }
}