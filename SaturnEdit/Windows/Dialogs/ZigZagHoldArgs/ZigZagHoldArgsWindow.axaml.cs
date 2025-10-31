using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.ZigZagHoldArgs;

public partial class ZigZagHoldArgsWindow : Window
{
    public ZigZagHoldArgsWindow()
    {
        InitializeComponent();
    }
    
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