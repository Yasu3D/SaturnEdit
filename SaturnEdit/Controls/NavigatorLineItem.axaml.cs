using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnData.Content.Cosmetics.NavigatorData;
using SaturnData.Utilities;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Controls;

public partial class NavigatorLineItem : UserControl
{
    public NavigatorLineItem()
    {
        InitializeComponent();
    }
    
    public NavigatorLine? NavigatorLine { get; private set; } = null;

    private bool blockEvents = false;
    
#region Methods
    public void SetItem(NavigatorLine navigatorLine)
    {
        NavigatorLine = navigatorLine;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }
#endregion Methods

#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (NavigatorLine == null) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxMessage.Text = NavigatorLine.Message;
            TextBoxAudioFilePath.Text = NavigatorLine.AudioPath;
            TextBoxMessageDuration.Text = (0.001f * NavigatorLine.Duration).ToString("0.000000", CultureInfo.InvariantCulture);
            ComboBoxExpression.SelectedIndex = (int)NavigatorLine.Expression;
            CheckBoxShowSkipButton.IsChecked = NavigatorLine.ShowSkipButton;

            IconFileNotFoundWarning.IsVisible = NavigatorLine.AudioPath != "" && !File.Exists(navigator.AbsoluteAudioPath(NavigatorLine));
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        base.OnUnloaded(e);
    }
    
    private void TextBoxMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorLine == null) return;

        string oldValue = NavigatorLine.Message;
        string newValue = TextBoxMessage.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { NavigatorLine.Message = value; }, oldValue, newValue));
    }
    
    private void TextBoxAudioPath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorLine == null) return;
        
        string oldValue = NavigatorLine.AudioPath;
        string newValue = TextBoxAudioFilePath.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { NavigatorLine.AudioPath = value; }, oldValue, newValue));
    }

    private async void ButtonPickAudioFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (NavigatorLine == null) return;
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
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", $"navigator{SaturnFileExtensionList.SaturnContentFile}");

                GenericEditOperation<string> op0 = new(value => { navigator.AbsoluteSourcePath = value; }, navigator.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(value => { NavigatorLine.AudioPath = value; }, NavigatorLine.AudioPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string rootDirectoryPath = ChartSystem.Entry.RootDirectory;
                string filePath = files[0].Path.LocalPath;
    
                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                (bool, string) result = await MainWindow.PromptFileMoveAndOverwrite(rootDirectoryPath, filePath);
                if (!result.Item1) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { NavigatorLine.AudioPath = value; }, NavigatorLine.AudioPath, result.Item2));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
    
    private void TextBoxMessageDuration_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorLine == null) return;
        
        try
        {
            float oldValue = NavigatorLine.Duration;
            float newValue = 1000 * Convert.ToSingle(TextBoxMessageDuration.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { NavigatorLine.Duration = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { NavigatorLine.Duration = value; }, NavigatorLine.Duration, 5000));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }

    private void ComboBoxExpression_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorLine == null) return;

        NavigatorExpression oldValue = NavigatorLine.Expression;
        NavigatorExpression newValue = (NavigatorExpression)ComboBoxExpression.SelectedIndex;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<NavigatorExpression>(value => { NavigatorLine.Expression = value; }, oldValue, newValue));
    }

    private void CheckBoxShowSkipButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (NavigatorLine == null) return;

        bool oldValue = NavigatorLine.ShowSkipButton;
        bool newValue = CheckBoxShowSkipButton.IsChecked ?? false;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<bool>(value => { NavigatorLine.ShowSkipButton = value; }, oldValue, newValue));
    }
#endregion UI Event Handlers
}