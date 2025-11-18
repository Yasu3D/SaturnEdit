using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.NewChart;

public partial class NewChartWindow : Window
{
    public NewChartWindow()
    {
        InitializeComponent();
        InitializeDialog();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    public float Tempo { get; private set; } = 120;
    public int MetreUpper { get; private set; } = 4;
    public int MetreLower { get; private set; } = 4;

    private bool blockEvents = false;

#region Methods
    private void InitializeDialog()
    {
        blockEvents = true;

        TextBoxTempo.Text = Tempo.ToString("0.000000", CultureInfo.InvariantCulture);
        TextBoxMetreUpper.Text = MetreUpper.ToString(CultureInfo.InvariantCulture);
        TextBoxMetreLower.Text = MetreLower.ToString(CultureInfo.InvariantCulture);
        
        blockEvents = false;
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
    
    private void TextBoxTempo_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxTempo == null) return;
        
        float newValue = 120;

        try
        {
            newValue = Convert.ToSingle(TextBoxTempo.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(0, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            if (ex is not FormatException or OverflowException)
            {
                Console.WriteLine(ex);
            }
        }

        Tempo = newValue;
        InitializeDialog();
    }
    
    private void TextBoxMetreUpper_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxMetreUpper == null) return;
        
        int newValue = 4;

        try
        {
            newValue = Convert.ToInt32(TextBoxMetreUpper.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(1, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            if (ex is not FormatException or OverflowException)
            {
                Console.WriteLine(ex);
            }
        }

        MetreUpper = newValue;
        InitializeDialog();
    }
    
    private void TextBoxMetreLower_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxMetreLower == null) return;
        
        int newValue = 4;

        try
        {
            newValue = Convert.ToInt32(TextBoxMetreLower.Text, CultureInfo.InvariantCulture);
            newValue = Math.Max(1, newValue);
        }
        catch (Exception ex)
        {
            // Don't throw.
            if (ex is not FormatException or OverflowException)
            {
                Console.WriteLine(ex);
            }
        }

        MetreLower = newValue;
        InitializeDialog();
    }
    
    private void ButtonCreate_OnClick(object? sender, RoutedEventArgs e)
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