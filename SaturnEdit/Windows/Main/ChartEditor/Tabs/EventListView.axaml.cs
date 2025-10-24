using System;
using Avalonia.Controls;
using Avalonia.Threading;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

// TODO.
public partial class EventListView : UserControl
{
    public EventListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockShortcutMoveItemUp.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
            TextBlockShortcutMoveItemDown.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
            TextBlockShortcutDeleteSelection.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        });
    }
#endregion System Event Delegates
}