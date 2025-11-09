using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectBookmarkData;

public partial class SelectBookmarkDataWindow : Window
{
    public SelectBookmarkDataWindow()
    {
        InitializeComponent();
        UpdateData();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    
    public string Message { get; set; } = "";
    public uint Color { get; set; } = 0xFFDDDDDD;

    private bool blockEvents = false;

#region Methods
    private void UpdateData()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxColor.Text = $"{Color - 0xFF000000:X6}";
            
            blockEvents = false;
        });
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
    
    private void TextBoxMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (TextBoxMessage == null) return;
        Message = TextBoxMessage.Text ?? "";
    }
    
    private void TextBoxColor_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxColor == null) return;
        
        Color = uint.TryParse(TextBoxColor.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
        UpdateData();
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