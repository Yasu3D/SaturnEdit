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

namespace SaturnEdit.Windows.Dialogs.Search;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        GenerateList();
    }
    
    private string query = "";
    
    private readonly HashSet<string> blacklist = 
    [
        "QuickCommands.Settings",
        "QuickCommands.VolumeMixer",
        "QuickCommands.Search",
        "List.MoveItemUp",  
        "List.MoveItemDown",
        "Proofreader.Run",
    ];
    
#region Methods
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
    private void TextBoxSearchQuery_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        query = TextBoxSearchQuery.Text ?? "";
        GenerateList();
    }

    private void ListBoxActions_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (ListBoxActions?.SelectedItem is not SearchForAnythingListItem item) return;

        bool close = true;
        
        if (item.Key == "File.New")
        {
            MainWindow.Instance?.ChartEditor.File_New();
            e.Handled = true;
        }
        else if (item.Key == "File.Open") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Open();
            e.Handled = true;
        }
        else if (item.Key == "File.Save") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Save();
            e.Handled = true;
        }
        else if (item.Key == "File.SaveAs") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_SaveAs();
            e.Handled = true;
        }
        else if (item.Key == "File.ReloadFromDisk") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_ReloadFromDisk();
            e.Handled = true;
        }
        else if (item.Key == "File.Export") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Export();
            e.Handled = true;
        }
        else if (item.Key == "File.RenderAsImage") 
        {
            MainWindow.Instance?.ChartEditor.File_RenderAsImage();
            e.Handled = true;
        }
        else if (item.Key == "File.Quit") 
        {
            MainWindow.Instance?.ChartEditor.File_Quit();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Undo")
        {
            UndoRedoSystem.ChartBranch.Undo();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Edit.Redo")
        {
            UndoRedoSystem.ChartBranch.Redo();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Edit.Cut")
        {
            MainWindow.Instance?.ChartEditor.Edit_Cut();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Copy") 
        {
            MainWindow.Instance?.ChartEditor.Edit_Copy();
            e.Handled = true;
        }
        else if (item.Key == "Edit.Paste") 
        {
            MainWindow.Instance?.ChartEditor.Edit_Paste();
            e.Handled = true;
        }
        else if (item.Key == "Edit.SelectAll") 
        {
            SelectionSystem.SelectAll();
            e.Handled = true;
        }
        else if (item.Key == "Edit.DeselectAll") 
        {
            SelectionSystem.DeselectAll();
            e.Handled = true;
        }
        else if (item.Key == "Edit.CheckerDeselect") 
        {
            SelectionSystem.CheckerDeselect();
            e.Handled = true;
        }
        else if (item.Key == "Edit.SelectByCriteria") 
        {
            MainWindow.Instance?.ChartEditor.Edit_SelectByCriteria();
            e.Handled = true;
        }
        else if (item.Key == "Navigate.MoveBeatForward") 
        {
            TimeSystem.Navigate_MoveBeatForward();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.MoveBeatBack") 
        {
            TimeSystem.Navigate_MoveBeatBack();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.MoveMeasureForward") 
        {
            TimeSystem.Navigate_MoveMeasureForward();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.MoveMeasureBack") 
        {
            TimeSystem.Navigate_MoveMeasureBack();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.JumpToNextObject") 
        {
            TimeSystem.Navigate_JumpToNextObject();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.JumpToPreviousObject") 
        {
            TimeSystem.Navigate_JumpToPreviousObject();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.IncreaseBeatDivision") 
        {
            TimeSystem.Navigate_IncreaseBeatDivision();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.DecreaseBeatDivision") 
        {
            TimeSystem.Navigate_DecreaseBeatDivision();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.DoubleBeatDivision") 
        {
            TimeSystem.Navigate_DoubleBeatDivision();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Navigate.HalveBeatDivision") 
        {
            TimeSystem.Navigate_HalveBeatDivision();
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.Touch") 
        {
            CursorSystem.SetType(CursorSystem.TouchNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.Chain") 
        {
            CursorSystem.SetType(CursorSystem.ChainNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.Hold") 
        {
            CursorSystem.SetType(CursorSystem.HoldNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.SlideClockwise") 
        {
            CursorSystem.SetType(CursorSystem.SlideClockwiseNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.SlideCounterclockwise") 
        {
            CursorSystem.SetType(CursorSystem.SlideCounterclockwiseNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.SnapForward") 
        {
            CursorSystem.SetType(CursorSystem.SnapForwardNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.SnapBackward") 
        {
            CursorSystem.SetType(CursorSystem.SnapBackwardNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.LaneShow") 
        {
            CursorSystem.SetType(CursorSystem.LaneShowNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.LaneHide") 
        {
            CursorSystem.SetType(CursorSystem.LaneHideNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.Sync") 
        {
            CursorSystem.SetType(CursorSystem.SyncNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.NoteType.MeasureLine") 
        {
            CursorSystem.SetType(CursorSystem.MeasureLineNote);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.BonusType.Normal") 
        {
            CursorSystem.BonusType = BonusType.Normal;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.BonusType.Bonus") 
        {
            CursorSystem.BonusType = BonusType.Bonus;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.BonusType.R") 
        {
            CursorSystem.BonusType = BonusType.R;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.JudgementType.Normal") 
        {
            CursorSystem.JudgementType = JudgementType.Normal;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.JudgementType.Fake") 
        {
            CursorSystem.JudgementType = JudgementType.Fake;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.JudgementType.Autoplay") 
        {
            CursorSystem.JudgementType = JudgementType.Autoplay;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.SweepDirection.Center") 
        {
            CursorSystem.Direction = LaneSweepDirection.Center;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.SweepDirection.Clockwise") 
        {
            CursorSystem.Direction = LaneSweepDirection.Clockwise;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.SweepDirection.Counterclockwise") 
        {
            CursorSystem.Direction = LaneSweepDirection.Counterclockwise;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.SweepDirection.Instant") 
        {
            CursorSystem.Direction = LaneSweepDirection.Instant;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.HoldPointRenderType.Hidden") 
        {
            CursorSystem.RenderType = HoldPointRenderType.Visible;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "NotePalette.HoldPointRenderType.Visible") 
        {
            CursorSystem.RenderType = HoldPointRenderType.Hidden;
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Cursor.IncreasePosition")
        {
            CursorSystem.Position++;
            e.Handled = true;
        }
        else if (item.Key == "Cursor.DecreasePosition")
        {
            CursorSystem.Position--;
            e.Handled = true;
        }
        else if (item.Key == "Cursor.IncreaseSize")
        {
            CursorSystem.Size++;
            e.Handled = true;
        }
        else if (item.Key == "Cursor.DecreaseSize")
        {
            CursorSystem.Size--;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Play") 
        {
            TimeSystem.PlaybackState = PlaybackState.Playing;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Pause") 
        {
            TimeSystem.PlaybackState = PlaybackState.Stopped;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.IncreasePlaybackSpeed") 
        {
            TimeSystem.PlaybackSpeed = Math.Min(300, TimeSystem.PlaybackSpeed + 5);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Editor.Playback.DecreasePlaybackSpeed") 
        {
            TimeSystem.PlaybackSpeed = Math.Max(5, TimeSystem.PlaybackSpeed - 5);
            e.Handled = true;
            close = false;
        }
        else if (item.Key == "Editor.Playback.LoopPlayback") 
        {
            SettingsSystem.AudioSettings.LoopPlayback = !SettingsSystem.AudioSettings.LoopPlayback;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.SetLoopMarkerStart") 
        {
            TimeSystem.LoopStart = TimeSystem.Timestamp.Time;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.SetLoopMarkerEnd") 
        {
            TimeSystem.LoopEnd = TimeSystem.Timestamp.Time;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Metronome") 
        {
            SettingsSystem.AudioSettings.Metronome = !SettingsSystem.AudioSettings.Metronome;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditType") 
        {
            EditorSystem.ToolBar_EditType();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditShape") 
        {
            EditorSystem.ToolBar_EditShape();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditBoth") 
        {
            EditorSystem.ToolBar_EditBoth();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.DeleteSelection") 
        {
            EditorSystem.ToolBar_Delete();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.Insert") 
        {
            EditorSystem.ToolBar_Insert();
            e.Handled = true;
        }
        else if (item.Key == "Editor.AutoMode") 
        {
            EditorSystem.ChangeEditMode();
            e.Handled = true;
        }
        else if (item.Key == "Editor.ObjectMode") 
        {
            EditorSystem.ChangeEditMode(EditorMode.ObjectMode);
            e.Handled = true;
        }
        else if (item.Key == "Editor.EditMode") 
        {
            EditorSystem.ChangeEditMode(EditorMode.EditMode);
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.TempoChange") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddTempoChangeEvent();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.MetreChange") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddMetreChangeEvent();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.TutorialMarker") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddTutorialMarkerEvent();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.SpeedChange") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddSpeedChangeEvent();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.VisibilityChange") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddVisibilityChangeEvent();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.StopEffect") 
        {
            EditorSystem.Insert_AddStopEffect();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.ReverseEffect") 
        {
            EditorSystem.Insert_AddReverseEffect();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.Bookmark")
        {
            MainWindow.Instance?.ChartEditor.ChartView_AddBookmark();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveBeatForward") 
        {
            EditorSystem.Transform_MoveSelectionBeatForward();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveBeatBack") 
        {
            EditorSystem.Transform_MoveSelectionBeatBack();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveMeasureForward") 
        {
            EditorSystem.Transform_MoveSelectionMeasureForward();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveMeasureBack") 
        {
            EditorSystem.Transform_MoveSelectionMeasureBack();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveClockwise") 
        {
            EditorSystem.Transform_MoveClockwise();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveCounterclockwise") 
        {
            EditorSystem.Transform_MoveCounterclockwise();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.IncreaseSize") 
        {
            EditorSystem.Transform_IncreaseSize();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.DecreaseSize") 
        {
            EditorSystem.Transform_DecreaseSize();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveClockwiseIterative") 
        {
            EditorSystem.Transform_MoveClockwiseIterative();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveCounterclockwiseIterative") 
        {
            EditorSystem.Transform_MoveCounterclockwiseIterative();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.IncreaseSizeIterative") 
        {
            EditorSystem.Transform_IncreaseSizeIterative();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.DecreaseSizeIterative") 
        {
            EditorSystem.Transform_DecreaseSizeIterative();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorHorizontal") 
        {
            EditorSystem.Transform_MirrorHorizontal();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorVertical") 
        {
            EditorSystem.Transform_MirrorVertical();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorCustom") 
        {
            EditorSystem.Transform_MirrorCustom();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.AdjustAxis") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AdjustAxis();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.FlipDirection") 
        {
            EditorSystem.Transform_FlipDirection();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ReverseSelection") 
        {
            EditorSystem.Transform_ReverseSelection();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ScaleSelection") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleSelection();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.OffsetChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_OffsetChart();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ScaleChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleChart();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_MirrorChart();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.ZigZagHold") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ZigZagHold();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.CutHold") 
        {
            EditorSystem.Convert_CutHold();
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.JoinHold") 
        {
            EditorSystem.Convert_JoinHold();
            e.Handled = true;
        }
        else if (item.Key == "Editor.IncreaseNoteSpeed")
        {
            SettingsSystem.RenderSettings.NoteSpeed = Math.Min(60, SettingsSystem.RenderSettings.NoteSpeed + 1);
            e.Handled = true;
        }
        else if (item.Key == "Editor.DecreaseNoteSpeed") 
        {
            SettingsSystem.RenderSettings.NoteSpeed = Math.Max(10, SettingsSystem.RenderSettings.NoteSpeed - 1);
            e.Handled = true;
        }
        else if (item.Key == "Editor.IncreaseBackgroundDim")
        {
            SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)Math.Min(4, (int)SettingsSystem.RenderSettings.BackgroundDim + 1);
            e.Handled = true;
        }
        else if (item.Key == "Editor.DecreaseBackgroundDim") 
        {
            SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)Math.Max(0, (int)SettingsSystem.RenderSettings.BackgroundDim - 1);
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowSpeedChanges")
        {
            SettingsSystem.RenderSettings.ShowSpeedChanges = !SettingsSystem.RenderSettings.ShowSpeedChanges;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowVisibilityChanges") 
        {
            SettingsSystem.RenderSettings.ShowVisibilityChanges = !SettingsSystem.RenderSettings.ShowVisibilityChanges;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowLaneToggleAnimations") 
        {
            SettingsSystem.RenderSettings.ShowLaneToggleAnimations = !SettingsSystem.RenderSettings.ShowLaneToggleAnimations;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.VisualizeLaneSweeps") 
        {
            SettingsSystem.RenderSettings.VisualizeLaneSweeps = !SettingsSystem.RenderSettings.VisualizeLaneSweeps;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowJudgeAreas") 
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = !SettingsSystem.RenderSettings.ShowJudgeAreas;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowMarvelousArea") 
        {
            SettingsSystem.RenderSettings.ShowMarvelousArea = !SettingsSystem.RenderSettings.ShowMarvelousArea;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowGreatArea") 
        {
            SettingsSystem.RenderSettings.ShowGreatArea = !SettingsSystem.RenderSettings.ShowGreatArea;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowGoodArea") 
        {
            SettingsSystem.RenderSettings.ShowGoodArea = !SettingsSystem.RenderSettings.ShowGoodArea;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.SaturnJudgeAreas")
        {
            SettingsSystem.RenderSettings.SaturnJudgeAreas = !SettingsSystem.RenderSettings.SaturnJudgeAreas;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Touch") 
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = !SettingsSystem.RenderSettings.ShowTouchNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SnapForward") 
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = !SettingsSystem.RenderSettings.ShowSnapForwardNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SnapBackward") 
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = !SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SlideClockwise") 
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SlideCounterclockwise") 
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Chain") 
        {
            SettingsSystem.RenderSettings.ShowChainNotes = !SettingsSystem.RenderSettings.ShowChainNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Hold") 
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = !SettingsSystem.RenderSettings.ShowHoldNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Sync") 
        {
            SettingsSystem.RenderSettings.ShowSyncNotes = !SettingsSystem.RenderSettings.ShowSyncNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.MeasureLine") 
        {
            SettingsSystem.RenderSettings.ShowMeasureLineNotes = !SettingsSystem.RenderSettings.ShowMeasureLineNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.BeatLine") 
        {
            SettingsSystem.RenderSettings.ShowBeatLineNotes = !SettingsSystem.RenderSettings.ShowBeatLineNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.LaneShow") 
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = !SettingsSystem.RenderSettings.ShowLaneShowNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.LaneHide") 
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = !SettingsSystem.RenderSettings.ShowLaneHideNotes;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.TempoChange") 
        {
            SettingsSystem.RenderSettings.ShowTempoChangeEvents = !SettingsSystem.RenderSettings.ShowTempoChangeEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.MetreChange") 
        {
            SettingsSystem.RenderSettings.ShowMetreChangeEvents = !SettingsSystem.RenderSettings.ShowMetreChangeEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SpeedChange") 
        {
            SettingsSystem.RenderSettings.ShowSpeedChangeEvents = !SettingsSystem.RenderSettings.ShowSpeedChangeEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.VisibilityChange") 
        {
            SettingsSystem.RenderSettings.ShowVisibilityChangeEvents = !SettingsSystem.RenderSettings.ShowVisibilityChangeEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.ReverseEffect") 
        {
            SettingsSystem.RenderSettings.ShowReverseEffectEvents = !SettingsSystem.RenderSettings.ShowReverseEffectEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.StopEffect") 
        {
            SettingsSystem.RenderSettings.ShowStopEffectEvents = !SettingsSystem.RenderSettings.ShowStopEffectEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.TutorialMarker") 
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkerEvents = !SettingsSystem.RenderSettings.ShowTutorialMarkerEvents;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.EventMarkers") 
        {
            SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = !SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.LaneToggleNotes") 
        {
            SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = !SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.HoldControlPoints") 
        {
            SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = !SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.Bookmarks") 
        {
            SettingsSystem.RenderSettings.HideBookmarksDuringPlayback = !SettingsSystem.RenderSettings.HideBookmarksDuringPlayback;
            e.Handled = true;
        }

        if (close)
        {
            Close();
        }
    }
#endregion UI Event Delegates
}