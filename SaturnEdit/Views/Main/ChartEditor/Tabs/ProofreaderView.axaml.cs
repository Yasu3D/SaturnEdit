using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Views.ProofreaderCriteria;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class ProofreaderView : UserControl
{
    public ProofreaderView()
    {
        InitializeComponent();
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