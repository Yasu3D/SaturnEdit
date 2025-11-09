using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectMetre;

public partial class SelectMetreWindow : Window
{
    public SelectMetreWindow()
    {
        InitializeComponent();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    
    public int Upper { get; set; } = 4;
    public int Lower { get; set; } = 4;

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
    
    private void TextBoxUpper_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (TextBoxUpper == null) return;

        try
        {
            Upper = Convert.ToInt32(TextBoxUpper.Text, CultureInfo.InvariantCulture);
            TextBoxUpper.Text = Upper.ToString("N0", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Upper = 4;
            TextBoxUpper.Text = Upper.ToString("N0", CultureInfo.InvariantCulture);
            
            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void TextBoxLower_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (TextBoxLower == null) return;

        try
        {
            Lower = Convert.ToInt32(TextBoxLower.Text, CultureInfo.InvariantCulture);
            TextBoxLower.Text = Lower.ToString("N0", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Lower = 4;
            TextBoxLower.Text = Lower.ToString("N0", CultureInfo.InvariantCulture);
            
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