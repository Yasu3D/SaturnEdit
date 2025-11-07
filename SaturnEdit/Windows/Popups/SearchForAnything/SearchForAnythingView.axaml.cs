using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnView;

namespace SaturnEdit.Windows.Popups.SearchForAnything;

public partial class SearchForAnythingView : UserControl
{
    public SearchForAnythingView()
    {
        InitializeComponent();
    }

    public event EventHandler? PopupClosed;
    
    private string query = "";
    private bool blockEvents = false;
    
    private readonly HashSet<string> blacklist = 
    [
        "QuickCommands.Settings",
        "QuickCommands.Search",
        "List.MoveItemUp",  
        "List.MoveItemDown",
        "Proofreader.Run",
    ];
    
#region Methods
    public void Show()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            query = "";
            
            TextBoxSearchQuery.Text = null;
            IsVisible = true;
            
            GenerateList();

            blockEvents = false;
        });
    }

    public void Hide()
    {
        IsVisible = false;
        PopupClosed?.Invoke(null, EventArgs.Empty);
    }

    private void GenerateList()
    {
        Dispatcher.UIThread.Post(() =>
        {
            List<KeyValuePair<string, Shortcut>> shortcuts = SettingsSystem.ShortcutSettings.ShortcutsFilteredByQuery(query).Where(x => !blacklist.Contains(x.Key)).ToList();
            
            for (int i = 0; i < shortcuts.Count; i++)
            {
                if (i < ListBoxActions.Items.Count)
                {
                    // Modify existing items
                    if (ListBoxActions.Items[i] is not SearchForAnythingListItem item) continue;

                    item.SetData(shortcuts[i].Key, shortcuts[i].Value);
                }
                else
                {
                    // Create new item
                    SearchForAnythingListItem item = new();
                    item.SetData(shortcuts[i].Key, shortcuts[i].Value);

                    ListBoxActions.Items.Add(item);
                }
            }

            for (int i = ListBoxActions.Items.Count - 1; i >= shortcuts.Count; i--)
            {
                if (ListBoxActions.Items[i] is not SearchForAnythingListItem item) continue;
            
                ListBoxActions.Items.Remove(item);
            }
        });
    }
#endregion Methods
    
#region UI Event Delegates
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        Hide();
    }
    
    private void TextBoxSearchQuery_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvents) return;

        query = TextBoxSearchQuery.Text ?? "";
        GenerateList();
    }

    private void ListBoxActions_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxActions?.SelectedItem is not SearchForAnythingListItem item) return;
        
        if (item.Key == "File.New")
        {
            MainWindow.Instance?.ChartEditor.File_New();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.Open") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Open();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.Save") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Save();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.SaveAs") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_SaveAs();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.ReloadFromDisk") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_ReloadFromDisk();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.Export") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Export();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.RenderAsImage") 
        {
            MainWindow.Instance?.ChartEditor.File_RenderAsImage();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "File.Quit") 
        {
            MainWindow.Instance?.ChartEditor.File_Quit();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Undo")
        {
            UndoRedoSystem.Undo();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Redo")
        {
            UndoRedoSystem.Redo();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Cut")
        {
            MainWindow.Instance?.ChartEditor.Edit_Cut();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Copy") 
        {
            MainWindow.Instance?.ChartEditor.Edit_Copy();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Paste") 
        {
            MainWindow.Instance?.ChartEditor.Edit_Paste();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.SelectAll") 
        {
            SelectionSystem.SelectAll();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.DeselectAll") 
        {
            SelectionSystem.DeselectAll();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.CheckerDeselect") 
        {
            SelectionSystem.CheckerDeselect();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Edit.SelectByCriteria") 
        {
            MainWindow.Instance?.ChartEditor.Edit_SelectByCriteria();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.MoveBeatForward") 
        {
            TimeSystem.Navigate_MoveBeatForward();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.MoveBeatBack") 
        {
            TimeSystem.Navigate_MoveBeatBack();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.MoveMeasureForward") 
        {
            TimeSystem.Navigate_MoveMeasureForward();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.MoveMeasureBack") 
        {
            TimeSystem.Navigate_MoveMeasureBack();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.JumpToNextObject") 
        {
            TimeSystem.Navigate_JumpToNextObject();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.JumpToPreviousObject") 
        {
            TimeSystem.Navigate_JumpToPreviousObject();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.IncreaseBeatDivision") 
        {
            TimeSystem.Navigate_IncreaseBeatDivision();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.DecreaseBeatDivision") 
        {
            TimeSystem.Navigate_DecreaseBeatDivision();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.DoubleBeatDivision") 
        {
            TimeSystem.Navigate_DoubleBeatDivision();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.HalveBeatDivision") 
        {
            TimeSystem.Navigate_HalveBeatDivision();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Touch") 
        {
            CursorSystem.SetType(CursorSystem.TouchNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Chain") 
        {
            CursorSystem.SetType(CursorSystem.ChainNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Hold") 
        {
            CursorSystem.SetType(CursorSystem.HoldNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SlideClockwise") 
        {
            CursorSystem.SetType(CursorSystem.SlideClockwiseNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SlideCounterclockwise") 
        {
            CursorSystem.SetType(CursorSystem.SlideCounterclockwiseNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SnapForward") 
        {
            CursorSystem.SetType(CursorSystem.SnapForwardNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SnapBackward") 
        {
            CursorSystem.SetType(CursorSystem.SnapBackwardNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.LaneShow") 
        {
            CursorSystem.SetType(CursorSystem.LaneShowNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.LaneHide") 
        {
            CursorSystem.SetType(CursorSystem.LaneHideNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Sync") 
        {
            CursorSystem.SetType(CursorSystem.SyncNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.MeasureLine") 
        {
            CursorSystem.SetType(CursorSystem.MeasureLineNote);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.BonusType.Normal") 
        {
            CursorSystem.BonusType = BonusType.Normal;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.BonusType.Bonus") 
        {
            CursorSystem.BonusType = BonusType.Bonus;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.BonusType.R") 
        {
            CursorSystem.BonusType = BonusType.R;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.JudgementType.Normal") 
        {
            CursorSystem.JudgementType = JudgementType.Normal;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.JudgementType.Fake") 
        {
            CursorSystem.JudgementType = JudgementType.Fake;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.JudgementType.Autoplay") 
        {
            CursorSystem.JudgementType = JudgementType.Autoplay;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Center") 
        {
            CursorSystem.Direction = LaneSweepDirection.Center;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Clockwise") 
        {
            CursorSystem.Direction = LaneSweepDirection.Clockwise;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Counterclockwise") 
        {
            CursorSystem.Direction = LaneSweepDirection.Counterclockwise;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Instant") 
        {
            CursorSystem.Direction = LaneSweepDirection.Instant;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.HoldPointRenderType.Hidden") 
        {
            CursorSystem.RenderType = HoldPointRenderType.Visible;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.HoldPointRenderType.Visible") 
        {
            CursorSystem.RenderType = HoldPointRenderType.Hidden;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Play") 
        {
            TimeSystem.PlaybackState = PlaybackState.Playing;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Pause") 
        {
            TimeSystem.PlaybackState = PlaybackState.Stopped;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.IncreasePlaybackSpeed") 
        {
            TimeSystem.PlaybackSpeed = Math.Min(300, TimeSystem.PlaybackSpeed + 5);
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.DecreasePlaybackSpeed") 
        {
            TimeSystem.PlaybackSpeed = Math.Max(5, TimeSystem.PlaybackSpeed - 5);
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.LoopPlayback") 
        {
            SettingsSystem.AudioSettings.LoopPlayback = !SettingsSystem.AudioSettings.LoopPlayback;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.SetLoopMarkerStart") 
        {
            TimeSystem.LoopStart = TimeSystem.Timestamp.Time;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.SetLoopMarkerEnd") 
        {
            TimeSystem.LoopEnd = TimeSystem.Timestamp.Time;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Metronome") 
        {
            SettingsSystem.AudioSettings.Metronome = !SettingsSystem.AudioSettings.Metronome;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditType") 
        {
            EditorSystem.ToolBar_EditType();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditShape") 
        {
            EditorSystem.ToolBar_EditShape();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditBoth") 
        {
            EditorSystem.ToolBar_EditBoth();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.DeleteSelection") 
        {
            EditorSystem.ToolBar_Delete();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.Insert") 
        {
            EditorSystem.ToolBar_Insert();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.AutoMode") 
        {
            EditorSystem.ChangeEditMode();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.ObjectMode") 
        {
            EditorSystem.ChangeEditMode(EditorMode.ObjectMode);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.EditMode") 
        {
            EditorSystem.ChangeEditMode(EditorMode.EditMode);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.TempoChange") 
        {
            EditorSystem.Insert_AddTempoChange();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.MetreChange") 
        {
            EditorSystem.Insert_AddMetreChange();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.TutorialMarker") 
        {
            EditorSystem.Insert_AddTutorialMarker();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.SpeedChange") 
        {
            EditorSystem.Insert_AddSpeedChange();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.VisibilityChange") 
        {
            EditorSystem.Insert_AddVisibilityChange();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.StopEffect") 
        {
            EditorSystem.Insert_AddStopEffect();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.ReverseEffect") 
        {
            EditorSystem.Insert_AddReverseEffect();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveBeatForward") 
        {
            EditorSystem.Transform_MoveSelectionBeatForward();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveBeatBack") 
        {
            EditorSystem.Transform_MoveSelectionBeatBack();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveMeasureForward") 
        {
            EditorSystem.Transform_MoveSelectionMeasureForward();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveMeasureBack") 
        {
            EditorSystem.Transform_MoveSelectionMeasureBack();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveClockwise") 
        {
            EditorSystem.Transform_MoveClockwise();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveCounterclockwise") 
        {
            EditorSystem.Transform_MoveCounterclockwise();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.IncreaseSize") 
        {
            EditorSystem.Transform_IncreaseSize();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.DecreaseSize") 
        {
            EditorSystem.Transform_DecreaseSize();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveClockwiseIterative") 
        {
            EditorSystem.Transform_MoveClockwiseIterative();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveCounterclockwiseIterative") 
        {
            EditorSystem.Transform_MoveCounterclockwiseIterative();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.IncreaseSizeIterative") 
        {
            EditorSystem.Transform_IncreaseSizeIterative();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.DecreaseSizeIterative") 
        {
            EditorSystem.Transform_DecreaseSizeIterative();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorHorizontal") 
        {
            EditorSystem.Transform_MirrorHorizontal();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorVertical") 
        {
            EditorSystem.Transform_MirrorVertical();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorCustom") 
        {
            EditorSystem.Transform_MirrorCustom();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.AdjustAxis") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AdjustAxis();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.FlipDirection") 
        {
            EditorSystem.Transform_FlipDirection();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ReverseSelection") 
        {
            EditorSystem.Transform_ReverseSelection();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ScaleSelection") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleSelection();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.OffsetChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_OffsetChart();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ScaleChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleChart();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_MirrorChart();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.ZigZagHold") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ZigZagHold();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.CutHold") 
        {
            EditorSystem.Convert_CutHold();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.JoinHold") 
        {
            EditorSystem.Convert_JoinHold();
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.IncreaseNoteSpeed")
        {
            SettingsSystem.RenderSettings.NoteSpeed = Math.Min(60, SettingsSystem.RenderSettings.NoteSpeed + 1);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.DecreaseNoteSpeed") 
        {
            SettingsSystem.RenderSettings.NoteSpeed = Math.Max(10, SettingsSystem.RenderSettings.NoteSpeed - 1);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.IncreaseBackgroundDim")
        {
            SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)Math.Min(4, (int)SettingsSystem.RenderSettings.BackgroundDim + 1);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.DecreaseBackgroundDim") 
        {
            SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)Math.Max(0, (int)SettingsSystem.RenderSettings.BackgroundDim - 1);
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowSpeedChanges")
        {
            SettingsSystem.RenderSettings.ShowSpeedChanges = !SettingsSystem.RenderSettings.ShowSpeedChanges;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowVisibilityChanges") 
        {
            SettingsSystem.RenderSettings.ShowVisibilityChanges = !SettingsSystem.RenderSettings.ShowVisibilityChanges;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowLaneToggleAnimations") 
        {
            SettingsSystem.RenderSettings.ShowLaneToggleAnimations = !SettingsSystem.RenderSettings.ShowLaneToggleAnimations;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.VisualizeLaneSweeps") 
        {
            SettingsSystem.RenderSettings.VisualizeLaneSweeps = !SettingsSystem.RenderSettings.VisualizeLaneSweeps;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowJudgeAreas") 
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = !SettingsSystem.RenderSettings.ShowJudgeAreas;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowMarvelousArea") 
        {
            SettingsSystem.RenderSettings.ShowMarvelousArea = !SettingsSystem.RenderSettings.ShowMarvelousArea;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowGreatArea") 
        {
            SettingsSystem.RenderSettings.ShowGreatArea = !SettingsSystem.RenderSettings.ShowGreatArea;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowGoodArea") 
        {
            SettingsSystem.RenderSettings.ShowGoodArea = !SettingsSystem.RenderSettings.ShowGoodArea;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.SaturnJudgeAreas")
        {
            SettingsSystem.RenderSettings.SaturnJudgeAreas = !SettingsSystem.RenderSettings.SaturnJudgeAreas;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Touch") 
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = !SettingsSystem.RenderSettings.ShowTouchNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SnapForward") 
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = !SettingsSystem.RenderSettings.ShowSnapForwardNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SnapBackward") 
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = !SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SlideClockwise") 
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SlideCounterclockwise") 
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Chain") 
        {
            SettingsSystem.RenderSettings.ShowChainNotes = !SettingsSystem.RenderSettings.ShowChainNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Hold") 
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = !SettingsSystem.RenderSettings.ShowHoldNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Sync") 
        {
            SettingsSystem.RenderSettings.ShowSyncNotes = !SettingsSystem.RenderSettings.ShowSyncNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.MeasureLine") 
        {
            SettingsSystem.RenderSettings.ShowMeasureLineNotes = !SettingsSystem.RenderSettings.ShowMeasureLineNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.BeatLine") 
        {
            SettingsSystem.RenderSettings.ShowBeatLineNotes = !SettingsSystem.RenderSettings.ShowBeatLineNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.LaneShow") 
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = !SettingsSystem.RenderSettings.ShowLaneShowNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.LaneHide") 
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = !SettingsSystem.RenderSettings.ShowLaneHideNotes;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.TempoChange") 
        {
            SettingsSystem.RenderSettings.ShowTempoChangeEvents = !SettingsSystem.RenderSettings.ShowTempoChangeEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.MetreChange") 
        {
            SettingsSystem.RenderSettings.ShowMetreChangeEvents = !SettingsSystem.RenderSettings.ShowMetreChangeEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SpeedChange") 
        {
            SettingsSystem.RenderSettings.ShowSpeedChangeEvents = !SettingsSystem.RenderSettings.ShowSpeedChangeEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.VisibilityChange") 
        {
            SettingsSystem.RenderSettings.ShowVisibilityChangeEvents = !SettingsSystem.RenderSettings.ShowVisibilityChangeEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.ReverseEffect") 
        {
            SettingsSystem.RenderSettings.ShowReverseEffectEvents = !SettingsSystem.RenderSettings.ShowReverseEffectEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.StopEffect") 
        {
            SettingsSystem.RenderSettings.ShowStopEffectEvents = !SettingsSystem.RenderSettings.ShowStopEffectEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.TutorialMarker") 
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkerEvents = !SettingsSystem.RenderSettings.ShowTutorialMarkerEvents;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.EventMarkers") 
        {
            SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = !SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.LaneToggleNotes") 
        {
            SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = !SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.HoldControlPoints") 
        {
            SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = !SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
            Hide();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.Bookmarks") 
        {
            SettingsSystem.RenderSettings.HideBookmarksDuringPlayback = !SettingsSystem.RenderSettings.HideBookmarksDuringPlayback;
            Hide();
            e.Handled = true;
        }
    }
#endregion UI Event Delegates
}