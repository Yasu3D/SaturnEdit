using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnData.Utilities;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class SystemSoundEditorView : UserControl
{
    public SystemSoundEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;

#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not SystemSound systemSound) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxArtist.Text = systemSound.Artist;

            TextBoxAudioLoginPath.Text                  = systemSound.AudioLoginPath;
            TextBoxAudioLoginCardSuccessPath.Text       = systemSound.AudioLoginCardSuccessPath;
            TextBoxAudioLoginCardErrorPath.Text         = systemSound.AudioLoginCardErrorPath;
            TextBoxAudioCycleModePath.Text              = systemSound.AudioCycleModePath;
            TextBoxAudioCycleFolderPath.Text            = systemSound.AudioCycleFolderPath;
            TextBoxAudioCycleSongPath.Text              = systemSound.AudioCycleSongPath;
            TextBoxAudioCycleOptionPath.Text            = systemSound.AudioCycleOptionPath;
            TextBoxAudioSelectOkPath.Text               = systemSound.AudioSelectOkPath;
            TextBoxAudioSelectBackPath.Text             = systemSound.AudioSelectBackPath;
            TextBoxAudioSelectDeniedPath.Text           = systemSound.AudioSelectDeniedPath;
            TextBoxAudioSelectDecidePath.Text           = systemSound.AudioSelectDecidePath;
            TextBoxAudioSelectPreviewSongPath.Text      = systemSound.AudioSelectPreviewSongPath;
            TextBoxAudioSelectStartSongPath.Text        = systemSound.AudioSelectStartSongPath;
            TextBoxAudioSelectStartSongAltPath.Text     = systemSound.AudioSelectStartSongAltPath;
            TextBoxAudioFavoriteAddPath.Text            = systemSound.AudioFavoriteAddPath;
            TextBoxAudioFavoriteRemovePath.Text         = systemSound.AudioFavoriteRemovePath;
            TextBoxAudioLevelUpCountPath.Text           = systemSound.AudioLevelUpCountPath;
            TextBoxAudioLevelUpFinishedPath.Text        = systemSound.AudioLevelUpFinishedPath;
            TextBoxAudioResultScoreCountPath.Text       = systemSound.AudioResultScoreCountPath;
            TextBoxAudioResultScoreFinishedPath.Text    = systemSound.AudioResultScoreFinishedPath;
            TextBoxAudioResultRateBadPath.Text          = systemSound.AudioResultRateBadPath;
            TextBoxAudioResultRateGoodPath.Text         = systemSound.AudioResultRateGoodPath;
            TextBoxAudioRhythmGameReadyPath.Text        = systemSound.AudioRhythmGameReadyPath;
            TextBoxAudioRhythmGameFailPath.Text         = systemSound.AudioRhythmGameFailPath;
            TextBoxAudioRhythmGameClearPath.Text        = systemSound.AudioRhythmGameClearPath;
            TextBoxAudioRhythmGameSpecialClearPath.Text = systemSound.AudioRhythmGameSpecialClearPath;
            TextBoxAudioTimerWarningPath.Text           = systemSound.AudioTimerWarningPath;
            TextBoxAudioTextboxAppearPath.Text          = systemSound.AudioTextboxAppearPath;
            TextBoxAudioMessageAlertPath.Text           = systemSound.AudioMessageAlertPath;

            IconFileNotFoundWarningAudioLogin.IsVisible                  = systemSound.AudioLoginPath                  != "" && !File.Exists(systemSound.AbsoluteAudioLoginPath);
            IconFileNotFoundWarningAudioLoginCardSuccess.IsVisible       = systemSound.AudioLoginCardSuccessPath       != "" && !File.Exists(systemSound.AbsoluteAudioLoginCardSuccessPath);
            IconFileNotFoundWarningAudioLoginCardError.IsVisible         = systemSound.AudioLoginCardErrorPath         != "" && !File.Exists(systemSound.AbsoluteAudioLoginCardErrorPath);
            IconFileNotFoundWarningAudioCycleMode.IsVisible              = systemSound.AudioCycleModePath              != "" && !File.Exists(systemSound.AbsoluteAudioCycleModePath);
            IconFileNotFoundWarningAudioCycleFolder.IsVisible            = systemSound.AudioCycleFolderPath            != "" && !File.Exists(systemSound.AbsoluteAudioCycleFolderPath);
            IconFileNotFoundWarningAudioCycleSong.IsVisible              = systemSound.AudioCycleSongPath              != "" && !File.Exists(systemSound.AbsoluteAudioCycleSongPath);
            IconFileNotFoundWarningAudioCycleOption.IsVisible            = systemSound.AudioCycleOptionPath            != "" && !File.Exists(systemSound.AbsoluteAudioCycleOptionPath);
            IconFileNotFoundWarningAudioSelectOk.IsVisible               = systemSound.AudioSelectOkPath               != "" && !File.Exists(systemSound.AbsoluteAudioSelectOkPath);
            IconFileNotFoundWarningAudioSelectBack.IsVisible             = systemSound.AudioSelectBackPath             != "" && !File.Exists(systemSound.AbsoluteAudioSelectBackPath);
            IconFileNotFoundWarningAudioSelectDenied.IsVisible           = systemSound.AudioSelectDeniedPath           != "" && !File.Exists(systemSound.AbsoluteAudioSelectDeniedPath);
            IconFileNotFoundWarningAudioSelectDecide.IsVisible           = systemSound.AudioSelectDecidePath           != "" && !File.Exists(systemSound.AbsoluteAudioSelectDecidePath);
            IconFileNotFoundWarningAudioSelectPreviewSong.IsVisible      = systemSound.AudioSelectPreviewSongPath      != "" && !File.Exists(systemSound.AbsoluteAudioSelectPreviewSongPath);
            IconFileNotFoundWarningAudioSelectStartSong.IsVisible        = systemSound.AudioSelectStartSongPath        != "" && !File.Exists(systemSound.AbsoluteAudioSelectStartSongPath);
            IconFileNotFoundWarningAudioSelectStartSongAlt.IsVisible     = systemSound.AudioSelectStartSongAltPath     != "" && !File.Exists(systemSound.AbsoluteAudioSelectStartSongAltPath);
            IconFileNotFoundWarningAudioFavoriteAdd.IsVisible            = systemSound.AudioFavoriteAddPath            != "" && !File.Exists(systemSound.AbsoluteAudioFavoriteAddPath);
            IconFileNotFoundWarningAudioFavoriteRemove.IsVisible         = systemSound.AudioFavoriteRemovePath         != "" && !File.Exists(systemSound.AbsoluteAudioFavoriteRemovePath);
            IconFileNotFoundWarningAudioLevelUpCount.IsVisible           = systemSound.AudioLevelUpCountPath           != "" && !File.Exists(systemSound.AbsoluteAudioLevelUpCountPath);
            IconFileNotFoundWarningAudioLevelUpFinished.IsVisible        = systemSound.AudioLevelUpFinishedPath        != "" && !File.Exists(systemSound.AbsoluteAudioLevelUpFinishedPath);
            IconFileNotFoundWarningAudioResultScoreCount.IsVisible       = systemSound.AudioResultScoreCountPath       != "" && !File.Exists(systemSound.AbsoluteAudioResultScoreCountPath);
            IconFileNotFoundWarningAudioResultScoreFinished.IsVisible    = systemSound.AudioResultScoreFinishedPath    != "" && !File.Exists(systemSound.AbsoluteAudioResultScoreFinishedPath);
            IconFileNotFoundWarningAudioResultRateBad.IsVisible          = systemSound.AudioResultRateBadPath          != "" && !File.Exists(systemSound.AbsoluteAudioResultRateBadPath);
            IconFileNotFoundWarningAudioResultRateGood.IsVisible         = systemSound.AudioResultRateGoodPath         != "" && !File.Exists(systemSound.AbsoluteAudioResultRateGoodPath);
            IconFileNotFoundWarningAudioRhythmGameReady.IsVisible        = systemSound.AudioRhythmGameReadyPath        != "" && !File.Exists(systemSound.AbsoluteAudioRhythmGameReadyPath);
            IconFileNotFoundWarningAudioRhythmGameFail.IsVisible         = systemSound.AudioRhythmGameFailPath         != "" && !File.Exists(systemSound.AbsoluteAudioRhythmGameFailPath);
            IconFileNotFoundWarningAudioRhythmGameClear.IsVisible        = systemSound.AudioRhythmGameClearPath        != "" && !File.Exists(systemSound.AbsoluteAudioRhythmGameClearPath);
            IconFileNotFoundWarningAudioRhythmGameSpecialClear.IsVisible = systemSound.AudioRhythmGameSpecialClearPath != "" && !File.Exists(systemSound.AbsoluteAudioRhythmGameSpecialClearPath);
            IconFileNotFoundWarningAudioTimerWarning.IsVisible           = systemSound.AudioTimerWarningPath           != "" && !File.Exists(systemSound.AbsoluteAudioTimerWarningPath);
            IconFileNotFoundWarningAudioTextboxAppear.IsVisible          = systemSound.AudioTextboxAppearPath          != "" && !File.Exists(systemSound.AbsoluteAudioTextboxAppearPath);
            IconFileNotFoundWarningAudioMessageAlert.IsVisible           = systemSound.AudioMessageAlertPath           != "" && !File.Exists(systemSound.AbsoluteAudioMessageAlertPath);

            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        
        base.OnUnloaded(e);
    }
    
    private void TextBoxArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not SystemSound systemSound) return;

        string oldValue = systemSound.Artist;
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }

        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { systemSound.Artist = value; }, oldValue, newValue));
    }

    private void TextBoxPath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not SystemSound systemSound) return;
        
        string oldValue = "";
        if      (textBox == TextBoxAudioLoginPath)                  { oldValue = systemSound.AudioLoginPath; }
        else if (textBox == TextBoxAudioLoginCardSuccessPath)       { oldValue = systemSound.AudioLoginCardSuccessPath; }
        else if (textBox == TextBoxAudioLoginCardErrorPath)         { oldValue = systemSound.AudioLoginCardErrorPath; }
        else if (textBox == TextBoxAudioCycleModePath)              { oldValue = systemSound.AudioCycleModePath; }
        else if (textBox == TextBoxAudioCycleFolderPath)            { oldValue = systemSound.AudioCycleFolderPath; }
        else if (textBox == TextBoxAudioCycleSongPath)              { oldValue = systemSound.AudioCycleSongPath; }
        else if (textBox == TextBoxAudioCycleOptionPath)            { oldValue = systemSound.AudioCycleOptionPath; }
        else if (textBox == TextBoxAudioSelectOkPath)               { oldValue = systemSound.AudioSelectOkPath; }
        else if (textBox == TextBoxAudioSelectBackPath)             { oldValue = systemSound.AudioSelectBackPath; }
        else if (textBox == TextBoxAudioSelectDeniedPath)           { oldValue = systemSound.AudioSelectDeniedPath; }
        else if (textBox == TextBoxAudioSelectDecidePath)           { oldValue = systemSound.AudioSelectDecidePath; }
        else if (textBox == TextBoxAudioSelectPreviewSongPath)      { oldValue = systemSound.AudioSelectPreviewSongPath; }
        else if (textBox == TextBoxAudioSelectStartSongPath)        { oldValue = systemSound.AudioSelectStartSongPath; }
        else if (textBox == TextBoxAudioSelectStartSongAltPath)     { oldValue = systemSound.AudioSelectStartSongAltPath; }
        else if (textBox == TextBoxAudioFavoriteAddPath)            { oldValue = systemSound.AudioFavoriteAddPath; }
        else if (textBox == TextBoxAudioFavoriteRemovePath)         { oldValue = systemSound.AudioFavoriteRemovePath; }
        else if (textBox == TextBoxAudioLevelUpCountPath)           { oldValue = systemSound.AudioLevelUpCountPath; }
        else if (textBox == TextBoxAudioLevelUpFinishedPath)        { oldValue = systemSound.AudioLevelUpFinishedPath; }
        else if (textBox == TextBoxAudioResultScoreCountPath)       { oldValue = systemSound.AudioResultScoreCountPath; }
        else if (textBox == TextBoxAudioResultScoreFinishedPath)    { oldValue = systemSound.AudioResultScoreFinishedPath; }
        else if (textBox == TextBoxAudioResultRateBadPath)          { oldValue = systemSound.AudioResultRateBadPath; }
        else if (textBox == TextBoxAudioResultRateGoodPath)         { oldValue = systemSound.AudioResultRateGoodPath; }
        else if (textBox == TextBoxAudioRhythmGameReadyPath)        { oldValue = systemSound.AudioRhythmGameReadyPath; }
        else if (textBox == TextBoxAudioRhythmGameFailPath)         { oldValue = systemSound.AudioRhythmGameFailPath; }
        else if (textBox == TextBoxAudioRhythmGameClearPath)        { oldValue = systemSound.AudioRhythmGameClearPath; }
        else if (textBox == TextBoxAudioRhythmGameSpecialClearPath) { oldValue = systemSound.AudioRhythmGameSpecialClearPath; }
        else if (textBox == TextBoxAudioTimerWarningPath)           { oldValue = systemSound.AudioTimerWarningPath; }
        else if (textBox == TextBoxAudioTextboxAppearPath)          { oldValue = systemSound.AudioTextboxAppearPath; }
        else if (textBox == TextBoxAudioMessageAlertPath)           { oldValue = systemSound.AudioMessageAlertPath; }
        
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }

        Action<string>? action = null;
        if      (textBox == TextBoxAudioLoginPath)                  { action = value => { systemSound.AudioLoginPath = value; }; }
        else if (textBox == TextBoxAudioLoginCardSuccessPath)       { action = value => { systemSound.AudioLoginCardSuccessPath = value; }; }
        else if (textBox == TextBoxAudioLoginCardErrorPath)         { action = value => { systemSound.AudioLoginCardErrorPath = value; }; }
        else if (textBox == TextBoxAudioCycleModePath)              { action = value => { systemSound.AudioCycleModePath = value; }; }
        else if (textBox == TextBoxAudioCycleFolderPath)            { action = value => { systemSound.AudioCycleFolderPath = value; }; }
        else if (textBox == TextBoxAudioCycleSongPath)              { action = value => { systemSound.AudioCycleSongPath = value; }; }
        else if (textBox == TextBoxAudioCycleOptionPath)            { action = value => { systemSound.AudioCycleOptionPath = value; }; }
        else if (textBox == TextBoxAudioSelectOkPath)               { action = value => { systemSound.AudioSelectOkPath = value; }; }
        else if (textBox == TextBoxAudioSelectBackPath)             { action = value => { systemSound.AudioSelectBackPath = value; }; }
        else if (textBox == TextBoxAudioSelectDeniedPath)           { action = value => { systemSound.AudioSelectDeniedPath = value; }; }
        else if (textBox == TextBoxAudioSelectDecidePath)           { action = value => { systemSound.AudioSelectDecidePath = value; }; }
        else if (textBox == TextBoxAudioSelectPreviewSongPath)      { action = value => { systemSound.AudioSelectPreviewSongPath = value; }; }
        else if (textBox == TextBoxAudioSelectStartSongPath)        { action = value => { systemSound.AudioSelectStartSongPath = value; }; }
        else if (textBox == TextBoxAudioSelectStartSongAltPath)     { action = value => { systemSound.AudioSelectStartSongAltPath = value; }; }
        else if (textBox == TextBoxAudioFavoriteAddPath)            { action = value => { systemSound.AudioFavoriteAddPath = value; }; }
        else if (textBox == TextBoxAudioFavoriteRemovePath)         { action = value => { systemSound.AudioFavoriteRemovePath = value; }; }
        else if (textBox == TextBoxAudioLevelUpCountPath)           { action = value => { systemSound.AudioLevelUpCountPath = value; }; }
        else if (textBox == TextBoxAudioLevelUpFinishedPath)        { action = value => { systemSound.AudioLevelUpFinishedPath = value; }; }
        else if (textBox == TextBoxAudioResultScoreCountPath)       { action = value => { systemSound.AudioResultScoreCountPath = value; }; }
        else if (textBox == TextBoxAudioResultScoreFinishedPath)    { action = value => { systemSound.AudioResultScoreFinishedPath = value; }; }
        else if (textBox == TextBoxAudioResultRateBadPath)          { action = value => { systemSound.AudioResultRateBadPath = value; }; }
        else if (textBox == TextBoxAudioResultRateGoodPath)         { action = value => { systemSound.AudioResultRateGoodPath = value; }; }
        else if (textBox == TextBoxAudioRhythmGameReadyPath)        { action = value => { systemSound.AudioRhythmGameReadyPath = value; }; }
        else if (textBox == TextBoxAudioRhythmGameFailPath)         { action = value => { systemSound.AudioRhythmGameFailPath = value; }; }
        else if (textBox == TextBoxAudioRhythmGameClearPath)        { action = value => { systemSound.AudioRhythmGameClearPath = value; }; }
        else if (textBox == TextBoxAudioRhythmGameSpecialClearPath) { action = value => { systemSound.AudioRhythmGameSpecialClearPath = value; }; }
        else if (textBox == TextBoxAudioTimerWarningPath)           { action = value => { systemSound.AudioTimerWarningPath = value; }; }
        else if (textBox == TextBoxAudioTextboxAppearPath)          { action = value => { systemSound.AudioTextboxAppearPath = value; }; }
        else if (textBox == TextBoxAudioMessageAlertPath)           { action = value => { systemSound.AudioMessageAlertPath = value; }; }
        if (action == null) return;

        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldValue, newValue));
    }

    private async void ButtonPickFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;
            if (CosmeticSystem.CosmeticItem is not SystemSound systemSound) return;

            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            string? oldAbsolutePath = null;
            string? oldLocalPath = null;
            if (button == ButtonAudioLoginPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioLoginPath;
                oldLocalPath = systemSound.AudioLoginPath;
            }
            else if (button == ButtonAudioLoginCardSuccessPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioLoginCardSuccessPath;
                oldLocalPath = systemSound.AudioLoginCardSuccessPath;
            }
            else if (button == ButtonAudioLoginCardErrorPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioLoginCardErrorPath;
                oldLocalPath = systemSound.AudioLoginCardErrorPath;
            }
            else if (button == ButtonAudioCycleModePath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioCycleModePath;
                oldLocalPath = systemSound.AudioCycleModePath;
            }
            else if (button == ButtonAudioCycleFolderPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioCycleFolderPath;
                oldLocalPath = systemSound.AudioCycleFolderPath;
            }
            else if (button == ButtonAudioCycleSongPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioCycleSongPath;
                oldLocalPath = systemSound.AudioCycleSongPath;
            }
            else if (button == ButtonAudioCycleOptionPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioCycleOptionPath;
                oldLocalPath = systemSound.AudioCycleOptionPath;
            }
            else if (button == ButtonAudioSelectOkPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectOkPath;
                oldLocalPath = systemSound.AudioSelectOkPath;
            }
            else if (button == ButtonAudioSelectBackPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectBackPath;
                oldLocalPath = systemSound.AudioSelectBackPath;
            }
            else if (button == ButtonAudioSelectDeniedPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectDeniedPath;
                oldLocalPath = systemSound.AudioSelectDeniedPath;
            }
            else if (button == ButtonAudioSelectDecidePath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectDecidePath;
                oldLocalPath = systemSound.AudioSelectDecidePath;
            }
            else if (button == ButtonAudioSelectPreviewSongPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectPreviewSongPath;
                oldLocalPath = systemSound.AudioSelectPreviewSongPath;
            }
            else if (button == ButtonAudioSelectStartSongPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectStartSongPath;
                oldLocalPath = systemSound.AudioSelectStartSongPath;
            }
            else if (button == ButtonAudioSelectStartSongAltPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioSelectStartSongAltPath;
                oldLocalPath = systemSound.AudioSelectStartSongAltPath;
            }
            else if (button == ButtonAudioFavoriteAddPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioFavoriteAddPath;
                oldLocalPath = systemSound.AudioFavoriteAddPath;
            }
            else if (button == ButtonAudioFavoriteRemovePath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioFavoriteRemovePath;
                oldLocalPath = systemSound.AudioFavoriteRemovePath;
            }
            else if (button == ButtonAudioLevelUpCountPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioLevelUpCountPath;
                oldLocalPath = systemSound.AudioLevelUpCountPath;
            }
            else if (button == ButtonAudioLevelUpFinishedPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioLevelUpFinishedPath;
                oldLocalPath = systemSound.AudioLevelUpFinishedPath;
            }
            else if (button == ButtonAudioResultScoreCountPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioResultScoreCountPath;
                oldLocalPath = systemSound.AudioResultScoreCountPath;
            }
            else if (button == ButtonAudioResultScoreFinishedPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioResultScoreFinishedPath;
                oldLocalPath = systemSound.AudioResultScoreFinishedPath;
            }
            else if (button == ButtonAudioResultRateBadPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioResultRateBadPath;
                oldLocalPath = systemSound.AudioResultRateBadPath;
            }
            else if (button == ButtonAudioResultRateGoodPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioResultRateGoodPath;
                oldLocalPath = systemSound.AudioResultRateGoodPath;
            }
            else if (button == ButtonAudioRhythmGameReadyPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioRhythmGameReadyPath;
                oldLocalPath = systemSound.AudioRhythmGameReadyPath;
            }
            else if (button == ButtonAudioRhythmGameFailPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioRhythmGameFailPath;
                oldLocalPath = systemSound.AudioRhythmGameFailPath;
            }
            else if (button == ButtonAudioRhythmGameClearPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioRhythmGameClearPath;
                oldLocalPath = systemSound.AudioRhythmGameClearPath;
            }
            else if (button == ButtonAudioRhythmGameSpecialClearPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioRhythmGameSpecialClearPath;
                oldLocalPath = systemSound.AudioRhythmGameSpecialClearPath;
            }
            else if (button == ButtonAudioTimerWarningPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioTimerWarningPath;
                oldLocalPath = systemSound.AudioTimerWarningPath;
            }
            else if (button == ButtonAudioTextboxAppearPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioTextboxAppearPath;
                oldLocalPath = systemSound.AudioTextboxAppearPath;
            }
            else if (button == ButtonAudioTextboxAppearPath)
            {
                oldAbsolutePath = systemSound.AbsoluteAudioMessageAlertPath;
                oldLocalPath = systemSound.AudioMessageAlertPath;
            }

            if (oldAbsolutePath == null) return;
            if (oldLocalPath == null) return;
            
            // Open file picker.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("Audio Files")
                    {
                        Patterns = ["*.wav", "*.mp3", "*.ogg", "*.flac"],
                    },
                ],
            });
            if (files.Count != 1) return;

            if (oldAbsolutePath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }

            Action<string>? action = null;
            if (button == ButtonAudioLoginPath)                       { action = value => { systemSound.AudioLoginPath = value; }; }
            else if (button == ButtonAudioLoginCardSuccessPath)       { action = value => { systemSound.AudioLoginCardSuccessPath = value; }; }
            else if (button == ButtonAudioLoginCardErrorPath)         { action = value => { systemSound.AudioLoginCardErrorPath = value; }; }
            else if (button == ButtonAudioCycleModePath)              { action = value => { systemSound.AudioCycleModePath = value; }; }
            else if (button == ButtonAudioCycleFolderPath)            { action = value => { systemSound.AudioCycleFolderPath = value; }; }
            else if (button == ButtonAudioCycleSongPath)              { action = value => { systemSound.AudioCycleSongPath = value; }; }
            else if (button == ButtonAudioCycleOptionPath)            { action = value => { systemSound.AudioCycleOptionPath = value; }; }
            else if (button == ButtonAudioSelectOkPath)               { action = value => { systemSound.AudioSelectOkPath = value; }; }
            else if (button == ButtonAudioSelectBackPath)             { action = value => { systemSound.AudioSelectBackPath = value; }; }
            else if (button == ButtonAudioSelectDeniedPath)           { action = value => { systemSound.AudioSelectDeniedPath = value; }; }
            else if (button == ButtonAudioSelectDecidePath)           { action = value => { systemSound.AudioSelectDecidePath = value; }; }
            else if (button == ButtonAudioSelectPreviewSongPath)      { action = value => { systemSound.AudioSelectPreviewSongPath = value; }; }
            else if (button == ButtonAudioSelectStartSongPath)        { action = value => { systemSound.AudioSelectStartSongPath = value; }; }
            else if (button == ButtonAudioSelectStartSongAltPath)     { action = value => { systemSound.AudioSelectStartSongAltPath = value; }; }
            else if (button == ButtonAudioFavoriteAddPath)            { action = value => { systemSound.AudioFavoriteAddPath = value; }; }
            else if (button == ButtonAudioFavoriteRemovePath)         { action = value => { systemSound.AudioFavoriteRemovePath = value; }; }
            else if (button == ButtonAudioLevelUpCountPath)           { action = value => { systemSound.AudioLevelUpCountPath = value; }; }
            else if (button == ButtonAudioLevelUpFinishedPath)        { action = value => { systemSound.AudioLevelUpFinishedPath = value; }; }
            else if (button == ButtonAudioResultScoreCountPath)       { action = value => { systemSound.AudioResultScoreCountPath = value; }; }
            else if (button == ButtonAudioResultScoreFinishedPath)    { action = value => { systemSound.AudioResultScoreFinishedPath = value; }; }
            else if (button == ButtonAudioResultRateBadPath)          { action = value => { systemSound.AudioResultRateBadPath = value; }; }
            else if (button == ButtonAudioResultRateGoodPath)         { action = value => { systemSound.AudioResultRateGoodPath = value; }; }
            else if (button == ButtonAudioRhythmGameReadyPath)        { action = value => { systemSound.AudioRhythmGameReadyPath = value; }; }
            else if (button == ButtonAudioRhythmGameFailPath)         { action = value => { systemSound.AudioRhythmGameFailPath = value; }; }
            else if (button == ButtonAudioRhythmGameClearPath)        { action = value => { systemSound.AudioRhythmGameClearPath = value; }; }
            else if (button == ButtonAudioRhythmGameSpecialClearPath) { action = value => { systemSound.AudioRhythmGameSpecialClearPath = value; }; }
            else if (button == ButtonAudioTimerWarningPath)           { action = value => { systemSound.AudioTimerWarningPath = value; }; }
            else if (button == ButtonAudioTextboxAppearPath)          { action = value => { systemSound.AudioTextboxAppearPath = value; }; }
            else if (button == ButtonAudioMessageAlertPath)           { action = value => { systemSound.AudioMessageAlertPath = value; }; }
            
            if (action == null) return;
            
            if (systemSound.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", $"system_music{SaturnFileExtensionList.SaturnContentFile}");

                GenericEditOperation<string> op0 = new(value => { systemSound.AbsoluteSourcePath = value; }, systemSound.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(action, oldLocalPath, Path.GetFileName(files[0].Path.LocalPath));

                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string rootDirectoryPath = Path.GetDirectoryName(systemSound.AbsoluteSourcePath) ?? "";
                string filePath = files[0].Path.LocalPath;
    
                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                (bool, string) result = await MainWindow.PromptFileMoveAndOverwrite(rootDirectoryPath, filePath);
                if (!result.Item1) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldLocalPath, result.Item2));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
#endregion UI Event Handlers
}