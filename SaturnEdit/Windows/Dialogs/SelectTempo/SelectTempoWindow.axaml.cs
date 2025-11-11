using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectTempo;

public partial class SelectTempoWindow : Window
{
    public SelectTempoWindow()
    {
        InitializeComponent();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    
    public float Tempo { get; set; } = 120.000000f;

#region Methods
    public void SetData(float tempo)
    {
        Tempo = tempo;
        
        TextBoxTempo.Text = Tempo.ToString("0.000000", CultureInfo.InvariantCulture);
    }
#endregion Methods
    
#region UI Event Delegates
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
    
    private void TextBoxTempo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (TextBoxTempo == null) return;

        try
        {
            Tempo = Convert.ToSingle(TextBoxTempo.Text, CultureInfo.InvariantCulture);
            TextBoxTempo.Text = Tempo.ToString("0.000000", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Tempo = 120;
            TextBoxTempo.Text = Tempo.ToString("0.000000", CultureInfo.InvariantCulture);
            
            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
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
#endregion UI Event Delegates
}