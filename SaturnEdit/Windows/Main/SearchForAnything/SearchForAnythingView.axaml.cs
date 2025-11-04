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

namespace SaturnEdit.Windows.Main.SearchForAnything;

public partial class SearchForAnythingView : UserControl
{
    public SearchForAnythingView()
    {
        InitializeComponent();
    }

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
        if (IsVisible)
        {
            IsVisible = false;
            return;
        }
        
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
        IsVisible = false;
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
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.Open") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Open();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.Save") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Save();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.SaveAs") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_SaveAs();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.ReloadFromDisk") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_ReloadFromDisk();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.Export") 
        {
            _ = MainWindow.Instance?.ChartEditor.File_Export();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.RenderAsImage") 
        {
            MainWindow.Instance?.ChartEditor.File_RenderAsImage();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "File.Quit") 
        {
            MainWindow.Instance?.ChartEditor.File_Quit();
            IsVisible = false;
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
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Edit.Copy") 
        {
            MainWindow.Instance?.ChartEditor.Edit_Copy();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Edit.Paste") 
        {
            MainWindow.Instance?.ChartEditor.Edit_Paste();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Edit.SelectAll") 
        {
            SelectionSystem.SelectAll();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Edit.DeselectAll") 
        {
            SelectionSystem.DeselectAll();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Edit.CheckerDeselect") 
        {
            SelectionSystem.CheckerDeselect();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Edit.SelectByCriteria") 
        {
            MainWindow.Instance?.ChartEditor.Edit_SelectByCriteria();
            IsVisible = false;
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
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Chain") 
        {
            CursorSystem.SetType(CursorSystem.ChainNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Hold") 
        {
            CursorSystem.SetType(CursorSystem.HoldNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SlideClockwise") 
        {
            CursorSystem.SetType(CursorSystem.SlideClockwiseNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SlideCounterclockwise") 
        {
            CursorSystem.SetType(CursorSystem.SlideCounterclockwiseNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SnapForward") 
        {
            CursorSystem.SetType(CursorSystem.SnapForwardNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.SnapBackward") 
        {
            CursorSystem.SetType(CursorSystem.SnapBackwardNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.LaneShow") 
        {
            CursorSystem.SetType(CursorSystem.LaneShowNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.LaneHide") 
        {
            CursorSystem.SetType(CursorSystem.LaneHideNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.Sync") 
        {
            CursorSystem.SetType(CursorSystem.SyncNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.NoteType.MeasureLine") 
        {
            CursorSystem.SetType(CursorSystem.MeasureLineNote);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.BonusType.Normal") 
        {
            CursorSystem.BonusType = BonusType.Normal;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.BonusType.Bonus") 
        {
            CursorSystem.BonusType = BonusType.Bonus;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.BonusType.R") 
        {
            CursorSystem.BonusType = BonusType.R;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.JudgementType.Normal") 
        {
            CursorSystem.JudgementType = JudgementType.Normal;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.JudgementType.Fake") 
        {
            CursorSystem.JudgementType = JudgementType.Fake;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.JudgementType.Autoplay") 
        {
            CursorSystem.JudgementType = JudgementType.Autoplay;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Center") 
        {
            CursorSystem.Direction = LaneSweepDirection.Center;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Clockwise") 
        {
            CursorSystem.Direction = LaneSweepDirection.Clockwise;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Counterclockwise") 
        {
            CursorSystem.Direction = LaneSweepDirection.Counterclockwise;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.SweepDirection.Instant") 
        {
            CursorSystem.Direction = LaneSweepDirection.Instant;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.HoldPointRenderType.Hidden") 
        {
            CursorSystem.RenderType = HoldPointRenderType.Visible;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "NotePalette.HoldPointRenderType.Visible") 
        {
            CursorSystem.RenderType = HoldPointRenderType.Hidden;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Play") 
        {
            TimeSystem.PlaybackState = PlaybackState.Playing;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Pause") 
        {
            TimeSystem.PlaybackState = PlaybackState.Stopped;
            IsVisible = false;
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
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.SetLoopMarkerStart") 
        {
            TimeSystem.LoopStart = TimeSystem.Timestamp.Time;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.SetLoopMarkerEnd") 
        {
            TimeSystem.LoopEnd = TimeSystem.Timestamp.Time;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Playback.Metronome") 
        {
            SettingsSystem.AudioSettings.Metronome = !SettingsSystem.AudioSettings.Metronome;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditType") 
        {
            EditorSystem.ToolBar_EditType();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditShape") 
        {
            EditorSystem.ToolBar_EditShape();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.EditBoth") 
        {
            EditorSystem.ToolBar_EditBoth();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.DeleteSelection") 
        {
            EditorSystem.ToolBar_Delete();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Toolbar.Insert") 
        {
            EditorSystem.ToolBar_Insert();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.AutoMode") 
        {
            EditorSystem.ChangeEditMode();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.ObjectMode") 
        {
            EditorSystem.ChangeEditMode(EditorMode.ObjectMode);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.EditMode") 
        {
            EditorSystem.ChangeEditMode(EditorMode.EditMode);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.TempoChange") 
        {
            EditorSystem.Insert_AddTempoChange();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.MetreChange") 
        {
            EditorSystem.Insert_AddMetreChange();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.TutorialMarker") 
        {
            EditorSystem.Insert_AddTutorialMarker();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.SpeedChange") 
        {
            EditorSystem.Insert_AddSpeedChange();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.VisibilityChange") 
        {
            EditorSystem.Insert_AddVisibilityChange();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.StopEffect") 
        {
            EditorSystem.Insert_AddStopEffect();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Insert.ReverseEffect") 
        {
            EditorSystem.Insert_AddReverseEffect();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveBeatForward") 
        {
            EditorSystem.Transform_MoveSelectionBeatForward();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveBeatBack") 
        {
            EditorSystem.Transform_MoveSelectionBeatBack();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveMeasureForward") 
        {
            EditorSystem.Transform_MoveSelectionMeasureForward();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveMeasureBack") 
        {
            EditorSystem.Transform_MoveSelectionMeasureBack();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveClockwise") 
        {
            EditorSystem.Transform_MoveClockwise();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveCounterclockwise") 
        {
            EditorSystem.Transform_MoveCounterclockwise();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.IncreaseSize") 
        {
            EditorSystem.Transform_IncreaseSize();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.DecreaseSize") 
        {
            EditorSystem.Transform_DecreaseSize();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveClockwiseIterative") 
        {
            EditorSystem.Transform_MoveClockwiseIterative();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MoveCounterclockwiseIterative") 
        {
            EditorSystem.Transform_MoveCounterclockwiseIterative();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.IncreaseSizeIterative") 
        {
            EditorSystem.Transform_IncreaseSizeIterative();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.DecreaseSizeIterative") 
        {
            EditorSystem.Transform_DecreaseSizeIterative();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorHorizontal") 
        {
            EditorSystem.Transform_MirrorHorizontal();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorVertical") 
        {
            EditorSystem.Transform_MirrorVertical();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorCustom") 
        {
            EditorSystem.Transform_MirrorCustom();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.AdjustAxis") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_AdjustAxis();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.FlipDirection") 
        {
            EditorSystem.Transform_FlipDirection();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ReverseSelection") 
        {
            EditorSystem.Transform_ReverseSelection();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ScaleSelection") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleSelection();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.OffsetChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_OffsetChart();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.ScaleChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ScaleChart();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Transform.MirrorChart") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_MirrorChart();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.ZigZagHold") 
        {
            MainWindow.Instance?.ChartEditor.ChartView_ZigZagHold();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.CutHold") 
        {
            EditorSystem.Convert_CutHold();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Convert.JoinHold") 
        {
            EditorSystem.Convert_JoinHold();
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.IncreaseNoteSpeed")
        {
            SettingsSystem.RenderSettings.NoteSpeed = Math.Min(60, SettingsSystem.RenderSettings.NoteSpeed + 1);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.DecreaseNoteSpeed") 
        {
            SettingsSystem.RenderSettings.NoteSpeed = Math.Max(10, SettingsSystem.RenderSettings.NoteSpeed - 1);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.IncreaseBackgroundDim")
        {
            SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)Math.Min(4, (int)SettingsSystem.RenderSettings.BackgroundDim + 1);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.DecreaseBackgroundDim") 
        {
            SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)Math.Max(0, (int)SettingsSystem.RenderSettings.BackgroundDim - 1);
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowSpeedChanges")
        {
            SettingsSystem.RenderSettings.ShowSpeedChanges = !SettingsSystem.RenderSettings.ShowSpeedChanges;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowVisibilityChanges") 
        {
            SettingsSystem.RenderSettings.ShowVisibilityChanges = !SettingsSystem.RenderSettings.ShowVisibilityChanges;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowLaneToggleAnimations") 
        {
            SettingsSystem.RenderSettings.ShowLaneToggleAnimations = !SettingsSystem.RenderSettings.ShowLaneToggleAnimations;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.VisualizeLaneSweeps") 
        {
            SettingsSystem.RenderSettings.VisualizeLaneSweeps = !SettingsSystem.RenderSettings.VisualizeLaneSweeps;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowJudgeAreas") 
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = !SettingsSystem.RenderSettings.ShowJudgeAreas;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowMarvelousArea") 
        {
            SettingsSystem.RenderSettings.ShowMarvelousArea = !SettingsSystem.RenderSettings.ShowMarvelousArea;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowGreatArea") 
        {
            SettingsSystem.RenderSettings.ShowGreatArea = !SettingsSystem.RenderSettings.ShowGreatArea;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ShowGoodArea") 
        {
            SettingsSystem.RenderSettings.ShowGoodArea = !SettingsSystem.RenderSettings.ShowGoodArea;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.SaturnJudgeAreas")
        {
            SettingsSystem.RenderSettings.SaturnJudgeAreas = !SettingsSystem.RenderSettings.SaturnJudgeAreas;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Touch") 
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = !SettingsSystem.RenderSettings.ShowTouchNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SnapForward") 
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = !SettingsSystem.RenderSettings.ShowSnapForwardNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SnapBackward") 
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = !SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SlideClockwise") 
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SlideCounterclockwise") 
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = !SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Chain") 
        {
            SettingsSystem.RenderSettings.ShowChainNotes = !SettingsSystem.RenderSettings.ShowChainNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Hold") 
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = !SettingsSystem.RenderSettings.ShowHoldNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.Sync") 
        {
            SettingsSystem.RenderSettings.ShowSyncNotes = !SettingsSystem.RenderSettings.ShowSyncNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.MeasureLine") 
        {
            SettingsSystem.RenderSettings.ShowMeasureLineNotes = !SettingsSystem.RenderSettings.ShowMeasureLineNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.BeatLine") 
        {
            SettingsSystem.RenderSettings.ShowBeatLineNotes = !SettingsSystem.RenderSettings.ShowBeatLineNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.LaneShow") 
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = !SettingsSystem.RenderSettings.ShowLaneShowNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.LaneHide") 
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = !SettingsSystem.RenderSettings.ShowLaneHideNotes;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.TempoChange") 
        {
            SettingsSystem.RenderSettings.ShowTempoChangeEvents = !SettingsSystem.RenderSettings.ShowTempoChangeEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.MetreChange") 
        {
            SettingsSystem.RenderSettings.ShowMetreChangeEvents = !SettingsSystem.RenderSettings.ShowMetreChangeEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.SpeedChange") 
        {
            SettingsSystem.RenderSettings.ShowSpeedChangeEvents = !SettingsSystem.RenderSettings.ShowSpeedChangeEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.VisibilityChange") 
        {
            SettingsSystem.RenderSettings.ShowVisibilityChangeEvents = !SettingsSystem.RenderSettings.ShowVisibilityChangeEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.ReverseEffect") 
        {
            SettingsSystem.RenderSettings.ShowReverseEffectEvents = !SettingsSystem.RenderSettings.ShowReverseEffectEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.StopEffect") 
        {
            SettingsSystem.RenderSettings.ShowStopEffectEvents = !SettingsSystem.RenderSettings.ShowStopEffectEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.ToggleVisibility.TutorialMarker") 
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkerEvents = !SettingsSystem.RenderSettings.ShowTutorialMarkerEvents;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.EventMarkers") 
        {
            SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = !SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.LaneToggleNotes") 
        {
            SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = !SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.HoldControlPoints") 
        {
            SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = !SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
            IsVisible = false;
            e.Handled = true;
        }
        else if (item.Key == "Editor.Settings.HideDuringPlayback.Bookmarks") 
        {
            SettingsSystem.RenderSettings.HideBookmarksDuringPlayback = !SettingsSystem.RenderSettings.HideBookmarksDuringPlayback;
            IsVisible = false;
            e.Handled = true;
        }
    }
#endregion UI Event Delegates
}