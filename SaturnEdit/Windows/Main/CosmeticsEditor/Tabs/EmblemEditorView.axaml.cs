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
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class EmblemEditorView : UserControl
{
    public EmblemEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not Emblem emblem) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBoxEmblemArtist.Text = emblem.Artist;
            TextBoxEmblemImagePath.Text = emblem.ImagePath;
            
            bool iconExists = File.Exists(emblem.AbsoluteImagePath);
            IconFileNotFoundWarning.IsVisible = emblem.AbsoluteImagePath != "" && !iconExists;
            
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
    
    private void TextBoxEmblemArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Emblem emblem) return;

        string oldValue = emblem.Artist;
        string newValue = TextBoxEmblemArtist.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { emblem.Artist = value; }, oldValue, newValue));
    }

    private void TextBoxEmblemImagePath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Emblem emblem) return;
        
        string oldValue = emblem.ImagePath;
        string newValue = TextBoxEmblemImagePath.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { emblem.ImagePath = value; }, oldValue, newValue));
    }

    private async void ButtonPickEmblemFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (CosmeticSystem.CosmeticItem is not Emblem emblem) return;
            
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
            
            if (emblem.AbsoluteImagePath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }
            
            if (emblem.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "emblem.toml");
                
                GenericEditOperation<string> op0 = new(value => { emblem.AbsoluteSourcePath = value; }, emblem.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(value => { emblem.ImagePath = value; }, emblem.ImagePath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(emblem.AbsoluteSourcePath) ?? "";
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { emblem.ImagePath = value; }, emblem.ImagePath, localPath));
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