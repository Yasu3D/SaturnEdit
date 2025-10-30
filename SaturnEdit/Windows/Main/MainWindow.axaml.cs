using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ChartEditor.SetMainWindow(this);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
        
        Closed += AudioSystem.OnClosed;

        if (Application.Current != null)
        {
            Application.Current.TryGetResource("ChartEditorChromeGradient", Application.Current.ActualThemeVariant, out object? resource);
            chartEditorChromeGradient = (IBrush?)resource;
            
            Application.Current.TryGetResource("StageEditorChromeGradient", Application.Current.ActualThemeVariant, out resource);
            stageEditorChromeGradient = (IBrush?)resource;
            
            Application.Current.TryGetResource("CosmeticsEditorChromeGradient", Application.Current.ActualThemeVariant, out resource);
            cosmeticsEditorChromeGradient = (IBrush?)resource;
        }
        
        SetTabs();
    }

    private readonly IBrush? chartEditorChromeGradient = null;
    private readonly IBrush? stageEditorChromeGradient = null;
    private readonly IBrush? cosmeticsEditorChromeGradient = null;
    
#region Methods
    public async void ShowSettingsWindow()
    {
        try
        {
            SettingsWindow settingsWindow = new();
            await settingsWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void SetTabs()
    {
        Dispatcher.UIThread.Post(() =>
        {
            bool chartEditor = TabChartEditor.IsChecked ?? false;
            bool stageEditor = TabStageEditor.IsChecked ?? false;
            bool cosmeticsEditor = TabCosmeticsEditor.IsChecked ?? false;
            
            ChartEditor.IsEnabled = chartEditor;
            ChartEditor.IsVisible = chartEditor;
            
            StageEditor.IsEnabled = stageEditor;
            StageEditor.IsVisible = stageEditor;
                
            CosmeticsEditor.IsEnabled = cosmeticsEditor;
            CosmeticsEditor.IsVisible = cosmeticsEditor;
            
            if (chartEditor)
            {
                RectangleChromeGradient.Fill = chartEditorChromeGradient;
            }
            else if (stageEditor)
            {
                RectangleChromeGradient.Fill = stageEditorChromeGradient;
            }
            else if (cosmeticsEditor)
            {
                RectangleChromeGradient.Fill = cosmeticsEditorChromeGradient;
            }
        });
    }
#endregion Methods

#region System Event Delegates
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ButtonUndo.IsEnabled = UndoRedoSystem.CanUndo;
            ButtonRedo.IsEnabled = UndoRedoSystem.CanRedo;
        });
    }
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockShortcutSearch.Text = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Search"].ToString();
            TextBlockShortcutSettings.Text = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToString();
            TextBlockShortcutUndo.Text = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToString();
            TextBlockShortcutRedo.Text = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToString();
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void EditorTabs_OnIsCheckedChanged(object? sender, RoutedEventArgs e) => SetTabs();

    private void ButtonSearch_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonSettings_OnClick(object? sender, RoutedEventArgs e) => ShowSettingsWindow();

    private void ButtonUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Undo();

    private void ButtonRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Redo();
#endregion UI Event Delegates
}