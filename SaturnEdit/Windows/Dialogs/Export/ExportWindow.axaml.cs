using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Notation.Serialization;

namespace SaturnEdit.Windows.Dialogs.Export;

public partial class ExportWindow : Window
{
    public ExportWindow()
    {
        InitializeComponent();
        OnArgsChanged();
        
        TextBoxWatermark.Watermark = DefaultExportWatermark;
    }

    public static string? DefaultExportWatermark => new NotationWriteArgs().ExportWatermark;
    public NotationWriteArgs NotationWriteArgs = new();
    
    public ExportDialogResult DialogResult = ExportDialogResult.Cancel;
    public enum ExportDialogResult
    {
        Cancel = 0,
        Export = 1,
    }
    
    private bool blockEvents = false;

    private void OnArgsChanged()
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
                GroupCommonArgs.IsVisible = false;
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
                GroupCommonArgs.IsVisible = true;
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
                GroupCommonArgs.IsVisible = true;
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
                GroupCommonArgs.IsVisible = true;
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
    }
    
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
        DialogResult = ExportDialogResult.Export;
        Close();
    }

    private void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        DialogResult = ExportDialogResult.Cancel;
        Close();
    }

    private void ButtonResetSettings_OnClick(object? sender, RoutedEventArgs e)
    {
        FormatVersion version = NotationWriteArgs.FormatVersion;
        NotationWriteArgs = new() { FormatVersion = version };
        
        OnArgsChanged();
    }
}