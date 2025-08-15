using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Systems;
using SaturnEdit.Windows.ProofreaderCriteria;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ProofreaderView : UserControl
{
    public ProofreaderView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        TextBlockShortcutRunProofreader.Text = SettingsSystem.ShortcutSettings.Shortcuts["Proofreader.Run"].ToString();
    }

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
            // ignored.
        }
    }
}