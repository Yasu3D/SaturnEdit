using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.SelectOffset;

public partial class SelectOffsetWindow : Window
{
    public SelectOffsetWindow()
    {
        InitializeComponent();
    }

    public int Offset { get; private set; } = 0;
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