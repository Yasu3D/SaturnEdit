using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Serialization;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.ImportArgs;

public partial class ImportArgsWindow : Window
{
    public ImportArgsWindow()
    {
        InitializeComponent();
        OnArgsChanged();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }
    
    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    
    public NotationReadArgs NotationReadArgs = new();
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
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;

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
        Result = ModalDialogResult.Primary;
        Close();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Result = ModalDialogResult.Cancel;
        Close();
    }

    private void ButtonResetSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        NotationReadArgs = new();
        OnArgsChanged();
    }
#endregion UI Event Delegates
}