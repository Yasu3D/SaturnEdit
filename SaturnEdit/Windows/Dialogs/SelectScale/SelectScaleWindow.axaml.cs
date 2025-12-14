using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectScale;

public partial class SelectScaleWindow : Window
{
    public SelectScaleWindow()
    {
        InitializeComponent();
    }

    public double Scale { get; private set; } = 1.0;
    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;

    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;
    
#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        keyDownEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        keyUpEventHandler = KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        keyDownEventHandler?.Dispose();
        keyUpEventHandler?.Dispose();
        
        base.OnUnloaded(e);
    }
    
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
    
    private void TextBoxScale_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (TextBoxScale == null) return;

        try
        {
            Scale = Convert.ToDouble(TextBoxScale.Text, CultureInfo.InvariantCulture);
            TextBoxScale.Text = Scale.ToString("0.000000", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Scale = 1;
            TextBoxScale.Text = Scale.ToString("0.000000", CultureInfo.InvariantCulture);
                
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
#endregion UI Event Handlers
}