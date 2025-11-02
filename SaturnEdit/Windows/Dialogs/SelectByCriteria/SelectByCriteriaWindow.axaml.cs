using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectByCriteria;

public partial class SelectByCriteriaWindow : Window
{
    public SelectByCriteriaWindow()
    {
        InitializeComponent();
        InitializeDialog();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;

    private bool blockEvents = false;

#region Methods
    private void InitializeDialog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            CheckBoxFilterSelection.IsChecked = SelectionSystem.SelectByCriteriaArgs.FilterSelection;
            
            SliderPosition.Value = SelectionSystem.SelectByCriteriaArgs.Position;
            SliderPositionVariance.Value = SelectionSystem.SelectByCriteriaArgs.PositionVariance;
            SliderSize.Value = SelectionSystem.SelectByCriteriaArgs.Size;
            SliderSizeVariance.Value = SelectionSystem.SelectByCriteriaArgs.SizeVariance;

            TextBlockPosition.Text = SelectionSystem.SelectByCriteriaArgs.Position.ToString();
            TextBlockPositionVariance.Text = SelectionSystem.SelectByCriteriaArgs.PositionVariance.ToString();
            TextBlockSize.Text = SelectionSystem.SelectByCriteriaArgs.Size.ToString();
            TextBlockSizeVariance.Text = SelectionSystem.SelectByCriteriaArgs.SizeVariance.ToString();
            
            CheckBoxTouch.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeTouchNotes;
            CheckBoxChain.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeChainNotes;
            CheckBoxHold.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeHoldNotes;
            CheckBoxSlideClockwise.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeSlideClockwiseNotes;
            CheckBoxSlideCounterclockwise.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeSlideCounterclockwiseNotes;
            CheckBoxSnapForward.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeSnapForwardNotes;
            CheckBoxSnapBackward.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeSnapBackwardNotes;
            CheckBoxLaneShow.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeLaneShowNotes;
            CheckBoxLaneHide.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeLaneHideNotes;
            CheckBoxSync.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeSyncNotes;
            CheckBoxMeasureLine.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeMeasureLineNotes;
            CheckBoxTempoChange.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeTempoChangeEvents;
            CheckBoxMetreChange.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeMetreChangeEvents;
            CheckBoxSpeedChange.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeSpeedChangeEvents;
            CheckBoxVisibilityChange.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeVisibilityChangeEvents;
            CheckBoxReverseEffect.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeReverseEffectEvents;
            CheckBoxStopEffect.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeStopEffectEvents;
            CheckBoxTutorialMarker.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeTutorialMarkerEvents;
            CheckBoxBookmark.IsChecked = SelectionSystem.SelectByCriteriaArgs.IncludeBookmarks;
            
            blockEvents = false;
        });
    }
#endregion Methods
    
#region UI Event Handlers
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;

        if (e.Key == Key.Escape)
        {
            Result = ModalDialogResult.Cancel;
            Close();
        }

        if (e.Key == Key.Enter)
        {
            Result = ModalDialogResult.Primary;
            Close();
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private void ButtonRunSelection_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Primary;
        Close();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Cancel;
        Close();
    }
    
    private void CheckBox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not CheckBox checkBox) return;

        bool value = checkBox.IsChecked ?? false;
        
        switch (checkBox.Name)
        {
            case "CheckBoxFilterSelection":
            {
                SelectionSystem.SelectByCriteriaArgs.FilterSelection = value;
                break;
            }
            
            case "CheckBoxTouch":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeTouchNotes = value;
                break;
            }
            
            case "CheckBoxChain":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeChainNotes = value;
                break;
            }
            
            case "CheckBoxHold":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeHoldNotes = value;
                break;
            }
            
            case "CheckBoxSlideClockwise":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeSlideClockwiseNotes = value;
                break;
            }
            
            case "CheckBoxSlideCounterclockwise":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeSlideCounterclockwiseNotes = value;
                break;
            }
            
            case "CheckBoxSnapForward":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeSnapForwardNotes = value;
                break;
            }
            
            case "CheckBoxSnapBackward":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeSnapBackwardNotes = value;
                break;
            }
            
            case "CheckBoxLaneShow":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeLaneShowNotes = value;
                break;
            }
            
            case "CheckBoxLaneHide":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeLaneHideNotes = value;
                break;
            }
            
            case "CheckBoxSync":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeSyncNotes = value;
                break;
            }
            
            case "CheckBoxMeasureLine":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeMeasureLineNotes = value;
                break;
            }
            
            case "CheckBoxTempoChange":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeTempoChangeEvents = value;
                break;
            }
            
            case "CheckBoxMetreChange":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeMetreChangeEvents = value;
                break;
            }
            
            case "CheckBoxSpeedChange":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeSpeedChangeEvents = value;
                break;
            }
            
            case "CheckBoxVisibilityChange":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeVisibilityChangeEvents = value;
                break;
            }
            
            case "CheckBoxReverseEffect":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeReverseEffectEvents = value;
                break;
            }
            
            case "CheckBoxStopEffect":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeStopEffectEvents = value;
                break;
            }
            
            case "CheckBoxTutorialMarker":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeTutorialMarkerEvents = value;
                break;
            }
            
            case "CheckBoxBookmark":
            {
                SelectionSystem.SelectByCriteriaArgs.IncludeBookmarks = value;
                break;
            }
        }
    }
    
    private void SliderPosition_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SliderPosition == null) return;
        
        SelectionSystem.SelectByCriteriaArgs.Position = (int)SliderPosition.Value;
        TextBlockPosition.Text = SelectionSystem.SelectByCriteriaArgs.Position.ToString();
    }

    private void SliderPositionVariance_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SliderPositionVariance == null) return;
        
        SelectionSystem.SelectByCriteriaArgs.PositionVariance = (int)SliderPositionVariance.Value;
        TextBlockPositionVariance.Text = SelectionSystem.SelectByCriteriaArgs.PositionVariance.ToString();
    }

    private void SliderSize_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SliderSize == null) return;
        
        SelectionSystem.SelectByCriteriaArgs.Size = (int)SliderSize.Value;
        TextBlockSize.Text = SelectionSystem.SelectByCriteriaArgs.Size.ToString();
    }

    private void SliderSizeVariance_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (SliderSizeVariance == null) return;
        
        SelectionSystem.SelectByCriteriaArgs.SizeVariance = (int)SliderSizeVariance.Value;
        TextBlockSizeVariance.Text = SelectionSystem.SelectByCriteriaArgs.SizeVariance.ToString();
    }
#endregion UI Event Handlers
}