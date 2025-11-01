using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

#region UI Event Delegates
    private void TextBoxScale_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (TextBoxScale == null) return;

        try
        {
            Scale = Convert.ToDouble(TextBoxScale.Text, CultureInfo.InvariantCulture);
            TextBoxScale.Text = Scale.ToString("F6", CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Scale = 1;
            TextBoxScale.Text = Scale.ToString("F6", CultureInfo.InvariantCulture);
                
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