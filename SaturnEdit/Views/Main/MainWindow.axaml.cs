using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Notation.Core;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;
using SaturnEdit.Views;
using SaturnEdit.Views.ChartEditor;
using SaturnEdit.Views.CosmeticsEditor;
using SaturnEdit.Views.StageEditor;

namespace SaturnEdit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        ChartEditor.Content = new ChartEditorView(this);
        StageEditor.Content = new StageEditorView(this);
        CosmeticsEditor.Content = new CosmeticsEditorView(this);
    }
    
    private void EditorTabs_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (ChartEditor != null)
        {
            ChartEditor.IsEnabled = button.Name == "TabChartEditor";
            ChartEditor.IsVisible = button.Name == "TabChartEditor";
        }
        
        if (StageEditor != null)
        {
            StageEditor.IsEnabled = button.Name == "TabStageEditor";
            StageEditor.IsVisible = button.Name == "TabStageEditor";
        }
        
        if (CosmeticsEditor != null)
        {
            CosmeticsEditor.IsEnabled = button.Name == "TabContentEditor";
            CosmeticsEditor.IsVisible = button.Name == "TabContentEditor";
        }
    }

    private void ButtonSearch_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonSettings_OnClick(object? sender, RoutedEventArgs e) => ShowSettingsWindow();

    private void ButtonUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonRedo_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    public async void ShowSettingsWindow()
    {
        try
        {
            SettingsWindow settingsWindow = new();
            await settingsWindow.ShowDialog(this);
        }
        catch (Exception e)
        {
            // ignored.
        }
    }
}