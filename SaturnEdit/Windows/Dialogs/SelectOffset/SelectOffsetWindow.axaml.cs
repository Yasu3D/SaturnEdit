using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectOffset;

public partial class SelectOffsetWindow : Window
{
    public SelectOffsetWindow()
    {
        InitializeComponent();
        UpdateValues();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    
    public int Offset => invert ? -AbsoluteOffset : AbsoluteOffset;
    private int AbsoluteOffset => measure * 1920 + beat * 1920 / division;
    
    private bool invert = false;
    private int measure = 0;
    private int beat = 0;
    private int division = 16;
    
    private bool blockEvents = false;

#region Methods
    private void UpdateValues()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            NumericUpDownMeasure.Value = measure;
            NumericUpDownBeat.Value = beat;
            NumericUpDownDivision.Value = division;
            
            bool oddDivision = 1920 % division != 0;
            IconOddDivisionWarning.IsVisible = oddDivision;

            ComboBoxDirection.SelectedIndex = invert ? 1 : 0;

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
        if (KeyDownBlacklist.IsInvalidState()) return;

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
    
    private void NumericUpDownBeat_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownBeat == null) return;
        
        beat = (int?)NumericUpDownBeat.Value ?? 0;
        
        if (beat == -1)
        {
            beat = division - 1;
            measure = Math.Max(0, measure - 1);
        }
        
        if (beat >= division)
        {
            beat = 0;
            measure += 1;
        }
        
        UpdateValues();
    }
    
    private void NumericUpDownDivision_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownDivision == null) return;
        
        division = (int?)NumericUpDownDivision.Value ?? 16;
        if (beat >= division)
        {
            beat = division - 1;
        }
        
        UpdateValues();
    }
    
    private void ComboBoxDirection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ComboBoxDirection == null) return;

        invert = ComboBoxDirection.SelectedIndex != 0;
        
        UpdateValues();
    }
    
    private void ButtonOk_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Primary;
        Close();
    }
    
    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Cancel;
        Close();
    }
#endregion UI Event Handlers
}