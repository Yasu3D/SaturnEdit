using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs.ProofreaderCriteria;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

// TODO.
public partial class ProofreaderView : UserControl
{
    public ProofreaderView()
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
            TextBlockShortcutRunProofreader.Text = SettingsSystem.ShortcutSettings.Shortcuts["Proofreader.Run"].ToString();
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
    private async void ButtonProofreadCriteria_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (VisualRoot is not Window window) return;
            
            ProofreaderCriteriaWindow proofreaderCriteriaWindow = new();
            await proofreaderCriteriaWindow.ShowDialog(window);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Delegates
}