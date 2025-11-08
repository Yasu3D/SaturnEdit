using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.ProofreaderCriteria;

public partial class ProofreaderCriteriaWindow : Window
{
    public ProofreaderCriteriaWindow()
    {
        InitializeComponent();
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; } = ModalDialogResult.Cancel;
    public Main.ChartEditor.Tabs.ProofreaderCriteria Criteria = new();

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
    
    private void CheckBox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox) return;
        
        if      (checkBox == CheckBoxStrictNoteSizeMer)       { Criteria.StrictNoteSizeMer       = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxStrictNoteSizeSat)       { Criteria.StrictNoteSizeSat       = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxStrictBonusTypeMer)      { Criteria.StrictBonusTypeMer      = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxOverlappingNotesStrict)  { Criteria.OverlappingNotesStrict  = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxOverlappingNotesLenient) { Criteria.OverlappingNotesLenient = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxEffectsOnLowers)         { Criteria.EffectsOnLowers         = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxInvalidEffects)          { Criteria.InvalidEffectsMer       = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxInvalidLaneToggles)      { Criteria.InvalidLaneToggles      = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxNotesDuringReverse)      { Criteria.NotesDuringReverse      = checkBox.IsChecked ?? false; }
        else if (checkBox == CheckBoxObjectsAfterChartEnd)    { Criteria.ObjectsAfterChartEnd    = checkBox.IsChecked ?? false; }
    }
    
    private void ButtonSave_OnClick(object? sender, RoutedEventArgs e)
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