using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Serialization;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.ImportArgs;

public partial class ImportArgsWindow : Window
{
    public ImportArgsWindow()
    {
        InitializeComponent();
        OnArgsChanged();
    }
    
    public NotationReadArgs NotationReadArgs = new();
    public ModalDialogResult DialogResult = ModalDialogResult.Cancel;
    private bool blockEvents = false;

#region System Event Delegates
    private void OnArgsChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
        
            CheckBoxSortCollections.IsChecked = NotationReadArgs.SortCollections;
            CheckBoxOptimizeHoldNotes.IsChecked = NotationReadArgs.OptimizeHoldNotes;
            CheckBoxInferClearThresholdFromDifficulty.IsChecked = NotationReadArgs.InferClearThresholdFromDifficulty;
            
            blockEvents = false;
        });
    }
#endregion System Event Delegates
    
#region UI Event Delegates
    private void CheckBoxSortCollections_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationReadArgs.SortCollections = CheckBoxSortCollections.IsChecked ?? true;
        OnArgsChanged();
    }

    private void CheckBoxOptimizeHoldNotes_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationReadArgs.OptimizeHoldNotes = CheckBoxOptimizeHoldNotes.IsChecked ?? true;
        OnArgsChanged();
    }

    private void CheckBoxInferClearThresholdFromDifficulty_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationReadArgs.InferClearThresholdFromDifficulty = CheckBoxInferClearThresholdFromDifficulty.IsChecked ?? true;
        OnArgsChanged();
    }
    
    private void ButtonOpen_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = ModalDialogResult.Primary;
        Close();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = ModalDialogResult.Cancel;
        Close();
    }

    private void ButtonResetSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        NotationReadArgs = new();
        OnArgsChanged();
    }
#endregion UI Event Delegates
}