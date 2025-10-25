using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.Windows.Dialogs;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        
        ChartSystem.EntryChanged += OnEntryChanged;
        OnEntryChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
        
        Closed += AudioSystem.OnClosed;
    }

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
#endregion Methods

#region System Event Delegates
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ButtonUndo.IsEnabled = UndoRedoSystem.CanUndo;
            ButtonRedo.IsEnabled = UndoRedoSystem.CanRedo;

            MenuItemChartEditorUndo.IsEnabled = UndoRedoSystem.CanUndo;
            MenuItemChartEditorRedo.IsEnabled = UndoRedoSystem.CanRedo;
        });
    }

    private void OnEntryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemChartEditorReloadFromDisk.IsEnabled = File.Exists(ChartSystem.Entry.ChartFile);
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
            
            MenuItemChartEditorNew.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.New"].ToKeyGesture();
            MenuItemChartEditorOpen.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Open"].ToKeyGesture();
            MenuItemChartEditorSave.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Save"].ToKeyGesture();
            MenuItemChartEditorSaveAs.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"].ToKeyGesture();
            MenuItemChartEditorReloadFromDisk.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"].ToKeyGesture();
            MenuItemChartEditorExport.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Export"].ToKeyGesture();
            MenuItemChartEditorRenderAsImage.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.RenderAsImage"].ToKeyGesture();
            MenuItemChartEditorSettings.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToKeyGesture();
            MenuItemChartEditorQuit.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"].ToKeyGesture();
            
            MenuItemChartEditorUndo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToKeyGesture();
            MenuItemChartEditorRedo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToKeyGesture(); 
            MenuItemChartEditorCut.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Cut"].ToKeyGesture();
            MenuItemChartEditorCopy.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Copy"].ToKeyGesture();
            MenuItemChartEditorPaste.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Paste"].ToKeyGesture();
            MenuItemChartEditorSelectAll.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectAll"].ToKeyGesture();
            MenuItemChartEditorDeselectAll.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.DeselectAll"].ToKeyGesture();
            MenuItemChartEditorCheckerDeselect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.CheckerDeselect"].ToKeyGesture();
            MenuItemChartEditorSelectSimilar.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.SelectSimilar"].ToKeyGesture();
            
            MenuItemChartEditorMoveBeatForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatForward"].ToKeyGesture();
            MenuItemChartEditorMoveBeatBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveBeatBack"].ToKeyGesture();
            MenuItemChartEditorMoveMeasureForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureForward"].ToKeyGesture();
            MenuItemChartEditorMoveMeasureBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.MoveMeasureBack"].ToKeyGesture();
            MenuItemChartEditorJumpToNextNote.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToNextNote"].ToKeyGesture();
            MenuItemChartEditorJumpToPreviousNote.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.JumpToPreviousNote"].ToKeyGesture();
            MenuItemChartEditorIncreaseBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.IncreaseBeatDivision"].ToKeyGesture();
            MenuItemChartEditorDecreaseBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DecreaseBeatDivision"].ToKeyGesture();
            MenuItemChartEditorDoubleBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.DoubleBeatDivision"].ToKeyGesture();
            MenuItemChartEditorHalveBeatDivision.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Navigate.HalveBeatDivision"].ToKeyGesture();
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
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

    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e) => ShowSettingsWindow();

    private void ButtonSearch_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonSettings_OnClick(object? sender, RoutedEventArgs e) => ShowSettingsWindow();

    private void ButtonUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
    
    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Undo();

    private void ButtonRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.Redo();
    
    private void MenuItemChartEditorToolWindows_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;
        UserControl? userControl = menuItem.Name switch
        {
            "MenuItemChartEditorChartView3D" => new ChartView3D(),
            "MenuItemChartEditorChartView2D" => new ChartView2D(),
            "MenuItemChartEditorChartViewTxt" => new ChartViewTxt(),
            "MenuItemChartEditorChartProperties" => new ChartPropertiesView(),
            "MenuItemChartEditorChartStatistics" => new ChartStatisticsView(),
            "MenuItemChartEditorProofreader" => new ProofreaderView(),
            "MenuItemChartEditorEventList" => new EventListView(),
            "MenuItemChartEditorLayerList" => new LayerListView(),
            "MenuItemChartEditorLaneToggleList" => new LaneToggleListView(),
            "MenuItemChartEditorBookmarkList" => new BookmarkListView(),
            "MenuItemChartEditorInspector" => new InspectorView(),
            "MenuItemChartEditorCursor" => new CursorView(),
            "MenuItemChartEditorAudioMixer" => new AudioMixerView(),
            "MenuItemChartEditorWaveform" => new WaveformView(),
            _ => null,
        };

        if (userControl == null) return;
        ChartEditor.CreateNewFloatingTool(userControl);
    }

    private void MenuItemChartEditorNew_OnClick(object? sender, RoutedEventArgs e) => ChartEditor.FileNew();

    private void MenuItemChartEditorOpen_OnClick(object? sender, RoutedEventArgs e) => _ = ChartEditor.FileOpen();

    private void MenuItemChartEditorSave_OnClick(object? sender, RoutedEventArgs e) => _ = ChartEditor.FileSave();

    private void MenuItemChartEditorSaveAs_OnClick(object? sender, RoutedEventArgs e) => _ = ChartEditor.FileSaveAs();

    private void MenuItemChartEditorReloadFromDisk_OnClick(object? sender, RoutedEventArgs e) => _ = ChartEditor.FileReloadFromDisk();

    private void MenuItemChartEditorExport_OnClick(object? sender, RoutedEventArgs e) => _ = ChartEditor.FileExport();

    private void MenuItemChartEditorQuit_OnClick(object? sender, RoutedEventArgs e) => ChartEditor.FileQuit();
    
    private void Window_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Control));

        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Play"]) && TimeSystem.PlaybackState != PlaybackState.Playing)
        {
            TimeSystem.PlaybackState = PlaybackState.Playing;
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Playback.Pause"]) && TimeSystem.PlaybackState != PlaybackState.Stopped)
        {
            TimeSystem.PlaybackState = PlaybackState.Stopped;
            e.Handled = true;
        }
    }
#endregion UI Event Delegates
}