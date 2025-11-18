using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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

    private bool blockEvents = false;
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs empty)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
        
            CheckBoxLowPerformanceMode.IsChecked = SettingsSystem.RenderSettings.LowPerformanceMode;
            
            NumericUpDownNoteSpeed.Value = SettingsSystem.RenderSettings.NoteSpeed / 10.0m;
            ComboBoxBackgroundDim.SelectedIndex = (int)SettingsSystem.RenderSettings.BackgroundDim;
            
            ComboBoxGuideLineType.SelectedIndex = (int)SettingsSystem.RenderSettings.GuideLineType;
            ComboBoxJudgementLineColor.SelectedIndex = (int)SettingsSystem.RenderSettings.JudgementLineColor;
            
            NumericUpDownHiddenOpacity.Value = SettingsSystem.RenderSettings.HiddenOpacity / 10.0m;
            ComboBoxUnitTickVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.UnitTickVisibility;

            ComboBoxBonusEffectVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.BonusEffectVisibility;
            ComboBoxRNoteEffectVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.RNoteEffectVisibility;
            NumericUpDownRNoteEffectOpacity.Value = SettingsSystem.RenderSettings.RNoteEffectOpacity / 10.0m;

            ComboBoxClearBackgroundVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.ClearBackgroundVisibility;

            ComboBoxDifficultyDisplayVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.DifficultyDisplayVisibility;
            ComboBoxLevelDisplayVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.LevelDisplayVisibility;
            ComboBoxTitleDisplayVisibility.SelectedIndex = (int)SettingsSystem.RenderSettings.TitleDisplayVisibility;
            
            ComboBoxNoteThickness.SelectedIndex = (int)SettingsSystem.RenderSettings.NoteThickness;
            
            ComboBoxNoteColorTouch.SelectedIndex = (int)SettingsSystem.RenderSettings.TouchNoteColor;
            ComboBoxNoteColorChain.SelectedIndex = (int)SettingsSystem.RenderSettings.ChainNoteColor;
            ComboBoxNoteColorHold.SelectedIndex = (int)SettingsSystem.RenderSettings.HoldNoteColor;
            ComboBoxNoteColorSlideClockwise.SelectedIndex = (int)SettingsSystem.RenderSettings.SlideClockwiseNoteColor;
            ComboBoxNoteColorSlideCounterclockwise.SelectedIndex = (int)SettingsSystem.RenderSettings.SlideCounterclockwiseNoteColor;
            ComboBoxNoteColorSnapForward.SelectedIndex = (int)SettingsSystem.RenderSettings.SnapForwardNoteColor;
            ComboBoxNoteColorSnapBackward.SelectedIndex = (int)SettingsSystem.RenderSettings.SnapBackwardNoteColor;

            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    private void CheckBoxLowPerformanceMode_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CheckBoxLowPerformanceMode == null) return;

        SettingsSystem.RenderSettings.LowPerformanceMode = CheckBoxLowPerformanceMode.IsChecked ?? false;
    }

    private void NumericUpDownNoteSpeed_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownNoteSpeed == null) return;

        SettingsSystem.RenderSettings.NoteSpeed = (int)(NumericUpDownNoteSpeed.Value * 10 ?? 30);
    }

    private void ComboBoxGuideLineType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxGuideLineType == null) return;

        SettingsSystem.RenderSettings.GuideLineType = (RenderSettings.GuideLineTypeOption)ComboBoxGuideLineType.SelectedIndex;
    }

    private void ComboBoxBackgroundDim_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxBackgroundDim == null) return;

        SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)ComboBoxBackgroundDim.SelectedIndex;
    }
    
    private void ComboBoxJudgementLineColor_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxJudgementLineColor == null) return;

        SettingsSystem.RenderSettings.JudgementLineColor = (RenderSettings.JudgementLineColorOption)ComboBoxJudgementLineColor.SelectedIndex;
    }
    
    private void NumericUpDownHiddenOpacity_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHiddenOpacity == null) return;

        SettingsSystem.RenderSettings.HiddenOpacity = (int)(NumericUpDownHiddenOpacity.Value * 10 ?? 10);
    }
    
    private void ComboBoxUnitTickVisibility_OnSelectionChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxUnitTickVisibility == null) return;

        SettingsSystem.RenderSettings.UnitTickVisibility = (RenderSettings.EffectVisibilityOption)ComboBoxUnitTickVisibility.SelectedIndex;
    }
    
    private void ComboBoxBonusEffectVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxBonusEffectVisibility == null) return;

        SettingsSystem.RenderSettings.BonusEffectVisibility = (RenderSettings.EffectVisibilityOption)ComboBoxBonusEffectVisibility.SelectedIndex;
    }
    
    private void ComboBoxRNoteEffectVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxRNoteEffectVisibility == null) return;

        SettingsSystem.RenderSettings.RNoteEffectVisibility = (RenderSettings.EffectVisibilityOption)ComboBoxRNoteEffectVisibility.SelectedIndex;
    }
    
    private void NumericUpDownRNoteEffectOpacity_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownRNoteEffectOpacity == null) return;

        SettingsSystem.RenderSettings.RNoteEffectOpacity = (int)(NumericUpDownRNoteEffectOpacity.Value * 10 ?? 30);
    }
    
    private void ComboBoxClearBackgroundVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxClearBackgroundVisibility == null) return;

        SettingsSystem.RenderSettings.ClearBackgroundVisibility = (RenderSettings.ClearBackgroundVisibilityOption)ComboBoxClearBackgroundVisibility.SelectedIndex;
    }
    
    private void ComboBoxDifficultyDisplayVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxDifficultyDisplayVisibility == null) return;

        SettingsSystem.RenderSettings.DifficultyDisplayVisibility = (RenderSettings.InterfaceVisibilityOption)ComboBoxDifficultyDisplayVisibility.SelectedIndex;
    }
        
    private void ComboBoxLevelDisplayVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxLevelDisplayVisibility == null) return;

        SettingsSystem.RenderSettings.LevelDisplayVisibility = (RenderSettings.InterfaceVisibilityOption)ComboBoxLevelDisplayVisibility.SelectedIndex;
    }

    private void ComboBoxTitleDisplayVisibility_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxTitleDisplayVisibility == null) return;

        SettingsSystem.RenderSettings.TitleDisplayVisibility = (RenderSettings.InterfaceVisibilityOption)ComboBoxTitleDisplayVisibility.SelectedIndex;
    }
    
    private void ComboBoxNoteThickness_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxNoteThickness == null) return;

        SettingsSystem.RenderSettings.NoteThickness = (RenderSettings.NoteThicknessOption)ComboBoxNoteThickness.SelectedIndex;
    }

    private void ComboBoxNoteColor_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
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
#endregion UI Event Handlers
}