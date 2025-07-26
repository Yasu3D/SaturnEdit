using System;
using Avalonia.Controls;
using Avalonia.Media;
using SaturnEdit.Systems;
using SaturnView;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class ChartStatisticsView : UserControl
{
    public ChartStatisticsView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        uint colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.TouchNoteColor);
        Color backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        Color borderColor = Color.FromUInt32(colorCode);
        GraphTouchNote.Background = new SolidColorBrush(backgroundColor);
        GraphTouchNote.BorderBrush = new SolidColorBrush(borderColor);
        
        colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.ChainNoteColor);
        backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        borderColor = Color.FromUInt32(colorCode);
        GraphChainNote.Background = new SolidColorBrush(backgroundColor);
        GraphChainNote.BorderBrush = new SolidColorBrush(borderColor);
        
        colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.HoldNoteColor);
        backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        borderColor = Color.FromUInt32(colorCode);
        GraphHoldNote.Background = new SolidColorBrush(backgroundColor);
        GraphHoldNote.BorderBrush = new SolidColorBrush(borderColor);
        
        colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor);
        backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        borderColor = Color.FromUInt32(colorCode);
        GraphSlideClockwiseNote.Background = new SolidColorBrush(backgroundColor);
        GraphSlideClockwiseNote.BorderBrush = new SolidColorBrush(borderColor);
        
        colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor);
        backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        borderColor = Color.FromUInt32(colorCode);
        GraphSlideCounterclockwiseNote.Background = new SolidColorBrush(backgroundColor);
        GraphSlideCounterclockwiseNote.BorderBrush = new SolidColorBrush(borderColor);
        
        colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.SnapForwardNoteColor);
        backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        borderColor = Color.FromUInt32(colorCode);
        GraphSnapForwardNote.Background = new SolidColorBrush(backgroundColor);
        GraphSnapForwardNote.BorderBrush = new SolidColorBrush(borderColor);
        
        colorCode = NoteColors.BaseNoteColorFromId((int)SettingsSystem.RenderSettings.SnapBackwardNoteColor);
        backgroundColor = Color.FromUInt32(colorCode - 0xA0000000);
        borderColor = Color.FromUInt32(colorCode);
        GraphSnapBackwardNote.Background = new SolidColorBrush(backgroundColor);
        GraphSnapBackwardNote.BorderBrush = new SolidColorBrush(borderColor);
    }
}