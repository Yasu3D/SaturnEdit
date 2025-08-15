using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SaturnData.Notation.Serialization;

namespace SaturnEdit.Windows.Dialogs.Export;

public partial class ExportWindow : Window
{
    public ExportWindow()
    {
        InitializeComponent();
    }
    
    public ExportDialogResult DialogResult = ExportDialogResult.Cancel;
    public enum ExportDialogResult
    {
        Cancel = 0,
        Export = 1,
    }
    
    private void ButtonExport_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = ExportDialogResult.Export;
        Close();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = ExportDialogResult.Cancel;
        Close();
    }
}