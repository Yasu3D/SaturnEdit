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