using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Serialization;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;

namespace SaturnEdit.Windows.Dialogs.ExportArgs;

public partial class ExportArgsWindow : Window
{
    public ExportArgsWindow()
    {
        InitializeComponent();
        OnArgsChanged();
        
        TextBoxWatermark.Watermark = DefaultExportWatermark;
        
        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
    }

    public ModalDialogResult Result { get; private set; }  = ModalDialogResult.Cancel;

    public static string? DefaultExportWatermark => new NotationWriteArgs().ExportWatermark;
    public NotationWriteArgs NotationWriteArgs = new();
    private bool blockEvents = false;
    
#region System Event Delegates
    private void OnArgsChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            ComboBoxFileType.SelectedIndex = (int)NotationWriteArgs.FormatVersion;
            
            // Only set Watermark Text if it's different to the default watermark text.
            TextBoxWatermark.Text = NotationWriteArgs.ExportWatermark == DefaultExportWatermark ? null : NotationWriteArgs.ExportWatermark;
            ComboBoxFakeNotes.SelectedIndex = (int)NotationWriteArgs.ConvertFakeNotes;
            ComboBoxAutoplayNotes.SelectedIndex = (int)NotationWriteArgs.ConvertAutoplayNotes;
            ComboBoxExtraLayers.SelectedIndex = (int)NotationWriteArgs.MergeExtraLayers;
            ComboBoxExtendedBonusTypes.SelectedIndex = (int)NotationWriteArgs.ConvertExtendedBonusTypes;
            ComboBoxWriteMusicPath.SelectedIndex = (int)NotationWriteArgs.WriteMusicFilePath;

            switch (NotationWriteArgs.FormatVersion)
            {
                case FormatVersion.Mer:
                {
                    GroupSatArgs.IsVisible = false;
                    OptionWatermark.IsVisible = false;

                    GroupBackwardsCompatibilityArgs.IsVisible = true;
                    OptionFakeNotes.IsVisible = true;
                    OptionAutoplayNotes.IsVisible = true;
                    OptionExtraLayers.IsVisible = true;
                    OptionExtendedBonusTypes.IsVisible = true;
                    
                    GroupMerArgs.IsVisible = true;
                    OptionWriteMusicPath.IsVisible = true;
                    break;
                }
                
                case FormatVersion.SatV1:
                {
                    GroupSatArgs.IsVisible = true;
                    OptionWatermark.IsVisible = true;

                    GroupBackwardsCompatibilityArgs.IsVisible = true;
                    OptionFakeNotes.IsVisible = true;
                    OptionAutoplayNotes.IsVisible = true;
                    OptionExtraLayers.IsVisible = true;
                    OptionExtendedBonusTypes.IsVisible = false;
                    
                    GroupMerArgs.IsVisible = false;
                    OptionWriteMusicPath.IsVisible = false;
                    break;
                }
                
                case FormatVersion.SatV2:
                {
                    GroupSatArgs.IsVisible = true;
                    OptionWatermark.IsVisible = true;

                    GroupBackwardsCompatibilityArgs.IsVisible = true;
                    OptionFakeNotes.IsVisible = true;
                    OptionAutoplayNotes.IsVisible = true;
                    OptionExtraLayers.IsVisible = false;
                    OptionExtendedBonusTypes.IsVisible = false;
                    
                    GroupMerArgs.IsVisible = false;
                    OptionWriteMusicPath.IsVisible = false;
                    break;
                }
                
                case FormatVersion.SatV3:
                {
                    GroupSatArgs.IsVisible = true;
                    OptionWatermark.IsVisible = true;

                    GroupBackwardsCompatibilityArgs.IsVisible = false;
                    OptionFakeNotes.IsVisible = false;
                    OptionAutoplayNotes.IsVisible = false;
                    OptionExtraLayers.IsVisible = false;
                    OptionExtendedBonusTypes.IsVisible = false;
                    
                    GroupMerArgs.IsVisible = false;
                    OptionWriteMusicPath.IsVisible = false;
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
            
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
    
    private void ComboBoxFileType_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationWriteArgs.FormatVersion = ComboBoxFileType.SelectedIndex switch
        {
            0 => FormatVersion.Mer,
            1 => FormatVersion.SatV1,
            2 => FormatVersion.SatV2,
            3 => FormatVersion.SatV3,
            _ => FormatVersion.Unknown,
        };
        OnArgsChanged();
    }
    
    private void TextBoxWatermark_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        NotationWriteArgs.ExportWatermark = string.IsNullOrEmpty(TextBoxWatermark.Text) ? DefaultExportWatermark : TextBoxWatermark.Text;
        OnArgsChanged();
    }

    private void ComboBoxFakeNotes_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        NotationWriteArgs.ConvertFakeNotes = (ConvertFakeNotesOption)ComboBoxFakeNotes.SelectedIndex;
        OnArgsChanged();
    }

    private void ComboBoxAutoplayNotes_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;

        NotationWriteArgs.ConvertAutoplayNotes = (ConvertAutoplayNotesOption)ComboBoxAutoplayNotes.SelectedIndex;
        OnArgsChanged();
    }

    private void ComboBoxExtraLayers_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationWriteArgs.MergeExtraLayers = (MergeExtraLayersOption)ComboBoxExtraLayers.SelectedIndex;
        OnArgsChanged();
    }

    private void ComboBoxExtendedBonusTypes_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationWriteArgs.ConvertExtendedBonusTypes = (ConvertExtendedBonusTypesOption)ComboBoxExtendedBonusTypes.SelectedIndex;
        OnArgsChanged();
    }

    private void ComboBoxWriteMusicPath_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        
        NotationWriteArgs.WriteMusicFilePath = (WriteMusicFilePathOption)ComboBoxWriteMusicPath.SelectedIndex;
        OnArgsChanged();
    }
    
    private void ButtonExport_OnClick(object? sender, RoutedEventArgs e)
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
        FormatVersion version = NotationWriteArgs.FormatVersion;
        NotationWriteArgs = new() { FormatVersion = version };
        
        OnArgsChanged();
    }
#endregion UI Event Delegates
}