using System;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;
using SaturnView;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartStatisticsView : UserControl
{
    public ChartStatisticsView()
    {
        InitializeComponent();
    }

#region Methods
    private void UpdateGraph()
    {
        double fullHeight = StackPanelGraph.Bounds.Height;

        int totalCount = 0;
        int touchCount = 0;
        int chainCount = 0;
        int holdCount = 0;
        int slideClockwiseCount = 0;
        int slideCounterclockwiseCount = 0;
        int snapForwardCount = 0;
        int snapBackwardCount = 0;

        foreach (Layer layer in ChartSystem.Chart.Layers)
        foreach (Note note in layer.Notes)
        {
            totalCount++;
            if (note is TouchNote)
            {
                touchCount++;
                continue;
            }

            if (note is ChainNote)
            {
                chainCount++;
                continue;
            }

            if (note is HoldNote)
            {
                holdCount++;
                continue;
            }

            if (note is SlideClockwiseNote)
            {
                slideClockwiseCount++;
                continue;
            }

            if (note is SlideCounterclockwiseNote)
            {
                slideCounterclockwiseCount++;
                continue;
            }

            if (note is SnapForwardNote)
            {
                snapForwardCount++;
                continue;
            }

            if (note is SnapBackwardNote)
            {
                snapBackwardCount++;
            }
        }

        Dispatcher.UIThread.Post(() =>
        {
            GraphTouchNote.IsVisible = touchCount > 0;
            GraphTouchNote.Height = fullHeight * ((double)touchCount / totalCount);

            GraphChainNote.IsVisible = chainCount > 0;
            GraphChainNote.Height = fullHeight * ((double)chainCount / totalCount);

            GraphHoldNote.IsVisible = holdCount > 0;
            GraphHoldNote.Height = fullHeight * ((double)holdCount / totalCount);

            GraphSlideClockwiseNote.IsVisible = slideClockwiseCount > 0;
            GraphSlideClockwiseNote.Height = fullHeight * ((double)slideClockwiseCount / totalCount);

            GraphSlideCounterclockwiseNote.IsVisible = slideCounterclockwiseCount > 0;
            GraphSlideCounterclockwiseNote.Height = fullHeight * ((double)slideCounterclockwiseCount / totalCount);

            GraphSnapForwardNote.IsVisible = snapForwardCount > 0;
            GraphSnapForwardNote.Height = fullHeight * ((double)snapForwardCount / totalCount);

            GraphSnapBackwardNote.IsVisible = snapBackwardCount > 0;
            GraphSnapBackwardNote.Height = fullHeight * ((double)snapBackwardCount / totalCount);
        });
    }

    private void UpdateStatistics()
    {
        int eventCount = 0;
        int tempoChangeCount = 0;
        int metreChangeCount = 0;
        int tutorialMarkerCount = 0;
        int speedChangeCount = 0;
        int visibilityChangeCount = 0;
        int reverseEffectCount = 0;
        int stopEffectCount = 0;

        foreach (Event @event in ChartSystem.Chart.Events)
        {
            eventCount++;
            if (@event is TempoChangeEvent) { tempoChangeCount++; continue; }
            if (@event is MetreChangeEvent) { metreChangeCount++; continue; }
            if (@event is TutorialMarkerEvent) { tutorialMarkerCount++; }
        }
        
        foreach (Layer layer in ChartSystem.Chart.Layers)
        foreach (Event @event in layer.Events)
        {
            eventCount++;
            if (@event is SpeedChangeEvent) { speedChangeCount++; continue; }
            if (@event is VisibilityChangeEvent) { visibilityChangeCount++; continue; }
            if (@event is ReverseEffectEvent) { reverseEffectCount++; continue; }
            if (@event is StopEffectEvent) { stopEffectCount++; }
        }

        int normalNoteCount = 0;
        int bonusNoteCount = 0;
        int rNoteCount = 0;
        
        int touchNoteCount = 0;
        int chainNoteCount = 0;
        int holdNoteCount = 0;
        int slideNoteCount = 0;
        int slideClockwiseNoteCount = 0;
        int slideCounterclockwiseNoteCount = 0;
        int snapNoteCount = 0;
        int snapForwardNoteCount = 0;
        int snapBackwardNoteCount = 0;
        int laneToggleNoteCount = 0;
        int laneShowNoteCount = 0;
        int laneHideNoteCount = 0;
        
        foreach (Layer layer in ChartSystem.Chart.Layers)
        foreach (Note note in layer.Notes)
        {
            if (note is IPlayable playable)
            {
                if (playable.BonusType == BonusType.Normal) normalNoteCount++;
                if (playable.BonusType == BonusType.Bonus) bonusNoteCount++;
                if (playable.BonusType == BonusType.R) rNoteCount++;
            }
            
            if (note is TouchNote) { touchNoteCount++; continue; }
            if (note is ChainNote) { chainNoteCount++; continue; }
            if (note is HoldNote)  { holdNoteCount++; continue; }
            
            if (note is SlideClockwiseNote)        { slideNoteCount++; slideClockwiseNoteCount++; continue; }
            if (note is SlideCounterclockwiseNote) { slideNoteCount++; slideCounterclockwiseNoteCount++; continue; }

            if (note is SnapForwardNote) { snapNoteCount++; snapForwardNoteCount++; continue; }
            if (note is SnapBackwardNote) { snapNoteCount++; snapBackwardNoteCount++; }
        }

        foreach (Note note in ChartSystem.Chart.LaneToggles)
        {
            if (note is LaneShowNote) { laneToggleNoteCount++; laneShowNoteCount++; continue; }
            if (note is LaneHideNote) { laneToggleNoteCount++; laneHideNoteCount++; }
        }

        int maxCombo = normalNoteCount + bonusNoteCount + rNoteCount;
        int effectiveNoteCount = normalNoteCount + bonusNoteCount + rNoteCount * 2;
        decimal scorePerNote = effectiveNoteCount == 0 ? 0 : 1_000_000.0m / effectiveNoteCount;
        
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockFileName.Text = Path.GetFileName(ChartSystem.Entry.ChartFile);

            TextBlockMaxCombo.Text = maxCombo.ToString();
            TextBlockScorePerNote.Text = scorePerNote == 0 ? "/" : scorePerNote.ToString("F2", CultureInfo.InvariantCulture);
            TextBlockScorePerRNote.Text = rNoteCount == 0 ? "/" : (scorePerNote * 2).ToString("F2", CultureInfo.InvariantCulture);
            
            TextBlockLayerCount.Text = ChartSystem.Chart.Layers.Count.ToString();
            
            TextBlockEventCount.Text = eventCount.ToString();
            TextBlockTempoChangeCount.Text = tempoChangeCount.ToString();
            TextBlockMetreChangeCount.Text = metreChangeCount.ToString();
            TextBlockTutorialMarkerCount.Text = tutorialMarkerCount.ToString();
            
            TextBlockSpeedChangeCount.Text = speedChangeCount.ToString();
            TextBlockVisibilityChangeCount.Text = visibilityChangeCount.ToString();
            TextBlockReverseEffectCount.Text = reverseEffectCount.ToString();
            TextBlockStopEffectCount.Text = stopEffectCount.ToString();
            
            TextBlockNormalNoteCount.Text = normalNoteCount.ToString();
            TextBlockBonusNoteCount.Text = bonusNoteCount.ToString();
            TextBlockRNoteCount.Text = rNoteCount.ToString();
            
            TextBlockTouchNoteCount.Text = touchNoteCount.ToString();
            TextBlockChainNoteCount.Text = chainNoteCount.ToString();
            TextBlockHoldNoteCount.Text = holdNoteCount.ToString();
            
            TextBlockSlideNoteCount.Text = slideNoteCount.ToString();
            TextBlockSlideClockwiseNoteCount.Text = slideClockwiseNoteCount.ToString();
            TextBlockSlideCounterclockwiseNoteCount.Text = slideCounterclockwiseNoteCount.ToString();
            
            TextBlockSnapNoteCount.Text = snapNoteCount.ToString();
            TextBlockSnapForwardNoteCount.Text = snapForwardNoteCount.ToString();
            TextBlockSnapBackwardNoteCount.Text = snapBackwardNoteCount.ToString();
            
            TextBlockLaneToggleNoteCount.Text = laneToggleNoteCount.ToString();
            TextBlockLaneShowNoteCount.Text = laneShowNoteCount.ToString();
            TextBlockLaneHideNoteCount.Text = laneHideNoteCount.ToString();
        });
    }
#endregion Methods

#region System Event Handlers
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        UpdateGraph();
        UpdateStatistics();
    }
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            uint colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.TouchNoteColor);
            Color backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            Color borderColor = Color.FromUInt32(colorCode);
            GraphTouchNote.Background = new SolidColorBrush(backgroundColor);
            GraphTouchNote.BorderBrush = new SolidColorBrush(borderColor);
            
            colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.ChainNoteColor);
            backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            borderColor = Color.FromUInt32(colorCode);
            GraphChainNote.Background = new SolidColorBrush(backgroundColor);
            GraphChainNote.BorderBrush = new SolidColorBrush(borderColor);
            
            colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.HoldNoteColor);
            backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            borderColor = Color.FromUInt32(colorCode);
            GraphHoldNote.Background = new SolidColorBrush(backgroundColor);
            GraphHoldNote.BorderBrush = new SolidColorBrush(borderColor);
            
            colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor);
            backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            borderColor = Color.FromUInt32(colorCode);
            GraphSlideClockwiseNote.Background = new SolidColorBrush(backgroundColor);
            GraphSlideClockwiseNote.BorderBrush = new SolidColorBrush(borderColor);
            
            colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor);
            backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            borderColor = Color.FromUInt32(colorCode);
            GraphSlideCounterclockwiseNote.Background = new SolidColorBrush(backgroundColor);
            GraphSlideCounterclockwiseNote.BorderBrush = new SolidColorBrush(borderColor);
            
            colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.SnapForwardNoteColor);
            backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            borderColor = Color.FromUInt32(colorCode);
            GraphSnapForwardNote.Background = new SolidColorBrush(backgroundColor);
            GraphSnapForwardNote.BorderBrush = new SolidColorBrush(borderColor);
            
            colorCode = NoteColors.AverageNoteColorFromId((int)SettingsSystem.RenderSettings.SnapBackwardNoteColor);
            backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
            borderColor = Color.FromUInt32(colorCode);
            GraphSnapBackwardNote.Background = new SolidColorBrush(backgroundColor);
            GraphSnapBackwardNote.BorderBrush = new SolidColorBrush(borderColor);
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        SizeChanged += Control_OnSizeChanged;
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        UndoRedoSystem.ChartBranch.OperationHistoryChanged -= ChartBranch_OnOperationHistoryChanged;
        SizeChanged -= Control_OnSizeChanged;
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateGraph();
    }
#endregion UI Event Handlers
}