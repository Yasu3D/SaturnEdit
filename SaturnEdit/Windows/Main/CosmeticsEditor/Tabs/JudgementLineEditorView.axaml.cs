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

public partial class JudgementLineColorEditorView : UserControl
{
    public JudgementLineColorEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not JudgementLineColor judgementLineColor) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBoxJudgementLineColorImagePath.Text = judgementLineColor.ImagePath;
            
            bool judgementLineColorExists = File.Exists(judgementLineColor.AbsoluteImagePath);
            IconFileNotFoundWarning.IsVisible = judgementLineColor.AbsoluteImagePath != "" && !judgementLineColorExists;
            
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

    private void TextBoxJudgementLineColorImagePath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not JudgementLineColor judgementLineColor) return;
        
        string oldValue = judgementLineColor.ImagePath;
        string newValue = TextBoxJudgementLineColorImagePath.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { judgementLineColor.ImagePath = value; }, oldValue, newValue));
    }

    private async void ButtonPickJudgementLineColorFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (CosmeticSystem.CosmeticItem is not JudgementLineColor judgementLineColor) return;
            
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
            
            // Open file picker.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("Image Files")
                    {
                        Patterns = ["*.png", "*.jpeg*", "*.jpg"],
                    },
                ],
            });
            if (files.Count != 1) return;
            
            if (judgementLineColor.AbsoluteImagePath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }
            
            if (judgementLineColor.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", $"judgement_line_color{SaturnFileExtensionList.SaturnContentFile}");

                GenericEditOperation<string> op0 = new(value => { judgementLineColor.AbsoluteSourcePath = value; }, judgementLineColor.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(value => { judgementLineColor.ImagePath = value; }, judgementLineColor.ImagePath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(judgementLineColor.AbsoluteSourcePath) ?? "";
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { judgementLineColor.ImagePath = value; }, judgementLineColor.ImagePath, localPath));
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