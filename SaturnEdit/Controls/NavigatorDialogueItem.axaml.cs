using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.PrimitiveOperations;

namespace SaturnEdit.Controls;

public partial class NavigatorDialogueItem : UserControl
{
    public NavigatorDialogueItem()
    {
        InitializeComponent();

        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
    }
    
    public NavigatorDialogue? NavigatorDialogue { get; private set; } = null;

    private bool blockEvents = false;
    
#region Methods
    public void SetItem(NavigatorDialogue navigatorDialogue)
    {
        NavigatorDialogue = navigatorDialogue;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }
#endregion Methods

#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (NavigatorDialogue == null) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxMessage.Text = NavigatorDialogue.Message;
            TextBoxAudioFilePath.Text = NavigatorDialogue.AudioPath;
            TextBoxMessageDuration.Text = (0.001f * NavigatorDialogue.Duration).ToString("0.000000", CultureInfo.InvariantCulture);
            ComboBoxExpression.SelectedIndex = (int)NavigatorDialogue.Expression;
            CheckBoxShowSkipButton.IsChecked = NavigatorDialogue.ShowSkipButton;

            IconFileNotFoundWarning.IsVisible = NavigatorDialogue.AudioPath != "" && !File.Exists(navigator.AbsoluteAudioPath(NavigatorDialogue));
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    private void TextBoxMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorDialogue == null) return;

        string oldValue = NavigatorDialogue.Message;
        string newValue = TextBoxMessage.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { NavigatorDialogue.Message = value; }, oldValue, newValue));
    }
    
    private void TextBoxAudioPath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorDialogue == null) return;
        
        string oldValue = NavigatorDialogue.AudioPath;
        string newValue = TextBoxAudioFilePath.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { NavigatorDialogue.AudioPath = value; }, oldValue, newValue));
    }

    private async void ButtonPickAudioFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (NavigatorDialogue == null) return;
            if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
            
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

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

            if (navigator.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "navigator.toml");

                GenericEditOperation<string> op0 = new(value => { navigator.AbsoluteSourcePath = value; }, navigator.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(value => { NavigatorDialogue.AudioPath = value; }, NavigatorDialogue.AudioPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string filename = Path.GetFileName(files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(Path.GetDirectoryName(navigator.AbsoluteSourcePath) ?? "", filename);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { NavigatorDialogue.AudioPath = value; }, NavigatorDialogue.AudioPath, filename));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }
    
    private void TextBoxMessageDuration_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorDialogue == null) return;
        
        try
        {
            float oldValue = NavigatorDialogue.Duration;
            float newValue = 1000 * Convert.ToSingle(TextBoxMessageDuration.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { NavigatorDialogue.Duration = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { NavigatorDialogue.Duration = value; }, NavigatorDialogue.Duration, 5000));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }

    private void ComboBoxExpression_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorDialogue == null) return;

        NavigatorExpression oldValue = NavigatorDialogue.Expression;
        NavigatorExpression newValue = (NavigatorExpression)ComboBoxExpression.SelectedIndex;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<NavigatorExpression>(value => { NavigatorDialogue.Expression = value; }, oldValue, newValue));
    }

    private void CheckBoxShowSkipButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorDialogue == null) return;

        bool oldValue = NavigatorDialogue.ShowSkipButton;
        bool newValue = CheckBoxShowSkipButton.IsChecked ?? false;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<bool>(value => { NavigatorDialogue.ShowSkipButton = value; }, oldValue, newValue));
    }
#endregion UI Event Handlers
}