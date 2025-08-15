using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Systems;
using SaturnView;

namespace SaturnEdit.Windows.Dialogs.Settings.Tabs;

public partial class SettingsRenderingView : UserControl
{
    public SettingsRenderingView()
    {
        InitializeComponent();
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private bool blockEvent = false;
    
    private void OnSettingsChanged(object? sender, EventArgs empty)
    {
        blockEvent = true;
        
        NumericUpDownRefreshRate.Value = SettingsSystem.RenderSettings.RefreshRate;
        CheckBoxLowPerformanceMode.IsChecked = SettingsSystem.RenderSettings.LowPerformanceMode;
        
        NumericUpDownNoteSpeed.Value = SettingsSystem.RenderSettings.NoteSpeed / 10.0m;
        ComboBoxGuideLineType.SelectedIndex = (int)SettingsSystem.RenderSettings.GuideLineType;
        ComboBoxBackgroundDim.SelectedIndex = (int)SettingsSystem.RenderSettings.BackgroundDim;
        
        CheckBoxIgnoreLaneToggleAnimations.IsChecked = SettingsSystem.RenderSettings.IgnoreLaneToggleAnimations;
        
        CheckBoxHideEventMarkersDuringPlayback.IsChecked = SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
        CheckBoxHideLaneToggleNotesDuringPlayback.IsChecked = SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
        CheckBoxHideHoldControlPointsDuringPlayback.IsChecked = SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;
        
        ComboBoxNoteThickness.SelectedIndex = (int)SettingsSystem.RenderSettings.NoteThickness;
        
        ComboBoxNoteColorTouch.SelectedIndex = (int)SettingsSystem.RenderSettings.TouchNoteColor;
        ComboBoxNoteColorChain.SelectedIndex = (int)SettingsSystem.RenderSettings.ChainNoteColor;
        ComboBoxNoteColorHold.SelectedIndex = (int)SettingsSystem.RenderSettings.HoldNoteColor;
        ComboBoxNoteColorSlideClockwise.SelectedIndex = (int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor;
        ComboBoxNoteColorSlideCounterclockwise.SelectedIndex = (int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor;
        ComboBoxNoteColorSnapForward.SelectedIndex = (int)SettingsSystem.RenderSettings.SnapForwardNoteColor;
        ComboBoxNoteColorSnapBackward.SelectedIndex = (int)SettingsSystem.RenderSettings.SnapBackwardNoteColor;

        blockEvent = false;
    }

    private void NumericUpDownRefreshRate_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvent) return;
        if (NumericUpDownRefreshRate == null) return;

        SettingsSystem.RenderSettings.RefreshRate = (int)(NumericUpDownRefreshRate.Value ?? 60);
    }

    private void CheckBoxLowPerformanceMode_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxLowPerformanceMode == null) return;

        SettingsSystem.RenderSettings.LowPerformanceMode = CheckBoxLowPerformanceMode.IsChecked ?? false;
    }

    private void NumericUpDownNoteSpeed_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvent) return;
        if (NumericUpDownNoteSpeed == null) return;

        SettingsSystem.RenderSettings.NoteSpeed = (int)(NumericUpDownNoteSpeed.Value * 10 ?? 30);
    }

    private void ComboBoxGuideLineType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (ComboBoxGuideLineType == null) return;

        SettingsSystem.RenderSettings.GuideLineType = (RenderSettings.GuideLineTypeOption)ComboBoxGuideLineType.SelectedIndex;
    }

    private void ComboBoxBackgroundDim_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (ComboBoxBackgroundDim == null) return;

        SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)ComboBoxBackgroundDim.SelectedIndex;
    }

    private void CheckBoxIgnoreLaneToggleAnimations_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxIgnoreLaneToggleAnimations == null) return;

        SettingsSystem.RenderSettings.IgnoreLaneToggleAnimations = CheckBoxIgnoreLaneToggleAnimations.IsChecked ?? false;
    }

    private void CheckBoxHideEventMarkersDuringPlayback_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxHideEventMarkersDuringPlayback == null) return;

        SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = CheckBoxHideEventMarkersDuringPlayback.IsChecked ?? false;
    }

    private void CheckBoxHideLaneToggleNotesDuringPlayback_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxHideLaneToggleNotesDuringPlayback == null) return;

        SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = CheckBoxHideLaneToggleNotesDuringPlayback.IsChecked ?? false;
    }

    private void CheckBoxHideHoldControlPointsDuringPlayback_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvent) return;
        if (CheckBoxHideHoldControlPointsDuringPlayback == null) return;

        SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = CheckBoxHideHoldControlPointsDuringPlayback.IsChecked ?? false;
    }

    private void ComboBoxNoteThickness_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (ComboBoxNoteThickness == null) return;

        SettingsSystem.RenderSettings.NoteThickness = (RenderSettings.NoteThicknessOption)ComboBoxNoteThickness.SelectedIndex;
    }

    private void ComboBoxNoteColor_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvent) return;
        if (sender is not ComboBox comboBox) return;

        RenderSettings.NoteColorOption color = (RenderSettings.NoteColorOption)comboBox.SelectedIndex;

        if (comboBox == ComboBoxNoteColorTouch)
        {
            SettingsSystem.RenderSettings.TouchNoteColor = color;
        }
        
        if (comboBox == ComboBoxNoteColorChain)
        {
            SettingsSystem.RenderSettings.ChainNoteColor = color;
        }
        
        if (comboBox == ComboBoxNoteColorHold)
        {
            SettingsSystem.RenderSettings.HoldNoteColor = color;
        }
        
        if (comboBox == ComboBoxNoteColorSlideClockwise)
        {
            SettingsSystem.RenderSettings.SlideClockwiseNoteColor = color;
        }
        
        if (comboBox == ComboBoxNoteColorSlideCounterclockwise)
        {
            SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor = color;
        }
        
        if (comboBox == ComboBoxNoteColorSnapForward)
        {
            SettingsSystem.RenderSettings.SnapForwardNoteColor = color;
        }
        
        if (comboBox == ComboBoxNoteColorSnapBackward)
        {
            SettingsSystem.RenderSettings.SnapBackwardNoteColor = color;
        }
    }
}