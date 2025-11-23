using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.PrimitiveOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class IconEditorView : UserControl
{
    public IconEditorView()
    {
        InitializeComponent();
        
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not Icon icon) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBoxIconArtist.Text = icon.Artist;
            TextBoxIconImagePath.Text = icon.ImagePath;
            
            bool iconExists = File.Exists(icon.AbsoluteImagePath);
            IconFileNotFoundWarning.IsVisible = icon.AbsoluteImagePath != "" && !iconExists;
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers
    
#region UI Event Handlers
    private void TextBoxIconArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Icon icon) return;

        string oldValue = icon.Artist;
        string newValue = TextBoxIconArtist.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(value => { icon.Artist = value; }, oldValue, newValue));
    }

    private void TextBoxIconImagePath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Icon icon) return;
        
        string oldValue = icon.ImagePath;
        string newValue = TextBoxIconImagePath.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(value => { icon.ImagePath = value; }, oldValue, newValue));
    }

    private async void ButtonPickIconFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (CosmeticSystem.CosmeticItem is not Icon icon) return;
            
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
            
            if (icon.AbsoluteImagePath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }
            
            if (icon.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "icon.toml");

                StringEditOperation op0 = new(value => { icon.AbsoluteSourcePath = value; }, icon.AbsoluteSourcePath, newSourcePath);
                StringEditOperation op1 = new(value => { icon.ImagePath = value; }, icon.ImagePath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(icon.AbsoluteSourcePath) ?? "";
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(value => { icon.ImagePath = value; }, icon.ImagePath, localPath));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Handlers
}