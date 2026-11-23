using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnData.Utilities;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class PlateEditorView : UserControl
{
    public PlateEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not Plate plate) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBoxPlateArtist.Text = plate.Artist;
            TextBoxPlateImagePath.Text = plate.ImagePath;
            
            bool plateExists = File.Exists(plate.AbsoluteImagePath);
            IconFileNotFoundWarning.IsVisible = plate.AbsoluteImagePath != "" && !plateExists;
            
            TextBoxTitleLabelColor.Text  = $"{plate.TitleLabelColor  - 0xFF000000:X6}";
            TextBoxLevelLabelColor.Text  = $"{plate.LevelLabelColor  - 0xFF000000:X6}";
            TextBoxPointLabelColor.Text  = $"{plate.PointLabelColor  - 0xFF000000:X6}";
            TextBoxRatingLabelColor.Text = $"{plate.RatingLabelColor - 0xFF000000:X6}";
            TextBoxTitleColor.Text = $"{plate.TitleColor - 0xFF000000:X6}";
            TextBoxLevelColor.Text = $"{plate.LevelColor - 0xFF000000:X6}";
            TextBoxPointColor.Text = $"{plate.PointColor - 0xFF000000:X6}";
            TextBoxNameColor.Text  = $"{plate.NameColor  - 0xFF000000:X6}";
                
            ColorPickerTitleLabelColor.Color  = plate.TitleLabelColor;
            ColorPickerLevelLabelColor.Color  = plate.LevelLabelColor;
            ColorPickerPointLabelColor.Color  = plate.PointLabelColor;
            ColorPickerRatingLabelColor.Color = plate.RatingLabelColor;
            ColorPickerTitleColor.Color = plate.TitleColor;
            ColorPickerLevelColor.Color = plate.LevelColor;
            ColorPickerPointColor.Color = plate.PointColor;
            ColorPickerNameColor.Color  = plate.NameColor;
            
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
    
    private void TextBoxPlateArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Plate plate) return;

        string oldValue = plate.Artist;
        string newValue = TextBoxPlateArtist.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { plate.Artist = value; }, oldValue, newValue));
    }

    private void TextBoxPlateImagePath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Plate plate) return;
        
        string oldValue = plate.ImagePath;
        string newValue = TextBoxPlateImagePath.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { plate.ImagePath = value; }, oldValue, newValue));
    }

    private async void ButtonPickPlateFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (CosmeticSystem.CosmeticItem is not Plate plate) return;
            
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
            
            if (plate.AbsoluteImagePath == files[0].Path.LocalPath)
            {
                // Refresh UI in case the file changed, but don't push unnecessary operation.
                CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
                return;
            }
            
            if (plate.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", $"plate{SaturnFileExtensionList.SaturnContentFile}");

                GenericEditOperation<string> op0 = new(value => { plate.AbsoluteSourcePath = value; }, plate.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(value => { plate.ImagePath = value; }, plate.ImagePath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string rootDirectoryPath = Path.GetDirectoryName(plate.AbsoluteSourcePath) ?? "";
                string filePath = files[0].Path.LocalPath;
    
                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                (bool, string) result = await MainWindow.PromptFileMoveAndOverwrite(rootDirectoryPath, filePath);
                if (!result.Item1) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { plate.ImagePath = value; }, plate.ImagePath, result.Item2));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }
    
    private void TextBoxColor_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not Plate plate) return;
        
        uint newValue = uint.TryParse(textBox.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
        
        uint oldValue = 0xFFFFFFFF;
        if      (textBox == TextBoxTitleLabelColor)  { oldValue = plate.TitleLabelColor; }
        else if (textBox == TextBoxLevelLabelColor)  { oldValue = plate.LevelLabelColor; }
        else if (textBox == TextBoxPointLabelColor)  { oldValue = plate.PointLabelColor; }
        else if (textBox == TextBoxRatingLabelColor) { oldValue = plate.RatingLabelColor; }
        else if (textBox == TextBoxTitleColor)       { oldValue = plate.TitleColor; }
        else if (textBox == TextBoxLevelColor)       { oldValue = plate.LevelColor; }
        else if (textBox == TextBoxPointColor)       { oldValue = plate.PointColor; }
        else if (textBox == TextBoxNameColor)        { oldValue = plate.NameColor; }
        if (oldValue == newValue) return;
        
        Action<uint>? action = null;
        if      (textBox == TextBoxTitleLabelColor)  { action = value => { plate.TitleLabelColor  = value; }; }
        else if (textBox == TextBoxLevelLabelColor)  { action = value => { plate.LevelLabelColor  = value; }; }
        else if (textBox == TextBoxPointLabelColor)  { action = value => { plate.PointLabelColor  = value; }; }
        else if (textBox == TextBoxRatingLabelColor) { action = value => { plate.RatingLabelColor = value; }; }
        else if (textBox == TextBoxTitleColor)       { action = value => { plate.TitleColor = value; }; }
        else if (textBox == TextBoxLevelColor)       { action = value => { plate.LevelColor = value; }; }
        else if (textBox == TextBoxPointColor)       { action = value => { plate.PointColor = value; }; }
        else if (textBox == TextBoxNameColor)        { action = value => { plate.NameColor  = value; }; }
        if (action == null) return;

        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(action, oldValue, newValue)); 
    }
    
    private void ColorPicker_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not SaturnColorPicker colorPicker) return;
        if (CosmeticSystem.CosmeticItem is not Plate plate) return;

        uint newValue = colorPicker.Color;
        
        uint oldValue = 0xFFFFFFFF;
        if      (colorPicker == ColorPickerTitleLabelColor)  { oldValue = plate.TitleLabelColor; }
        else if (colorPicker == ColorPickerLevelLabelColor)  { oldValue = plate.LevelLabelColor; }
        else if (colorPicker == ColorPickerPointLabelColor)  { oldValue = plate.PointLabelColor; }
        else if (colorPicker == ColorPickerRatingLabelColor) { oldValue = plate.RatingLabelColor; }
        else if (colorPicker == ColorPickerTitleColor)       { oldValue = plate.TitleColor; }
        else if (colorPicker == ColorPickerLevelColor)       { oldValue = plate.LevelColor; }
        else if (colorPicker == ColorPickerPointColor)       { oldValue = plate.PointColor; }
        else if (colorPicker == ColorPickerNameColor)        { oldValue = plate.NameColor; }
        if (oldValue == newValue) return;
        
        Action<uint>? action = null;
        if      (colorPicker == ColorPickerTitleLabelColor)  { action = value => { plate.TitleLabelColor  = value; }; }
        else if (colorPicker == ColorPickerLevelLabelColor)  { action = value => { plate.LevelLabelColor  = value; }; }
        else if (colorPicker == ColorPickerPointLabelColor)  { action = value => { plate.PointLabelColor  = value; }; }
        else if (colorPicker == ColorPickerRatingLabelColor) { action = value => { plate.RatingLabelColor = value; }; }
        else if (colorPicker == ColorPickerTitleColor)       { action = value => { plate.TitleColor = value; }; }
        else if (colorPicker == ColorPickerLevelColor)       { action = value => { plate.LevelColor = value; }; }
        else if (colorPicker == ColorPickerPointColor)       { action = value => { plate.PointColor = value; }; }
        else if (colorPicker == ColorPickerNameColor)        { action = value => { plate.NameColor  = value; }; }
        if (action == null) return;

        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(action, oldValue, newValue)); 
    }
    
    private void ColorPicker_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not SaturnColorPicker colorPicker) return;

        blockEvents = true;
        
        uint newValue = colorPicker.Color;
        string newText = $"{newValue - 0xFF000000:X6}";
        if      (colorPicker == ColorPickerTitleLabelColor)  { TextBoxTitleLabelColor.Text  = newText; }
        else if (colorPicker == ColorPickerLevelLabelColor)  { TextBoxLevelLabelColor.Text  = newText; }
        else if (colorPicker == ColorPickerPointLabelColor)  { TextBoxPointLabelColor.Text  = newText; }
        else if (colorPicker == ColorPickerRatingLabelColor) { TextBoxRatingLabelColor.Text = newText; }
        else if (colorPicker == ColorPickerTitleColor)       { TextBoxTitleColor.Text = newText; }
        else if (colorPicker == ColorPickerLevelColor)       { TextBoxLevelColor.Text = newText; }
        else if (colorPicker == ColorPickerPointColor)       { TextBoxPointColor.Text = newText; }
        else if (colorPicker == ColorPickerNameColor)        { TextBoxNameColor.Text  = newText; }
        
        blockEvents = false;
    }
#endregion UI Event Handlers
}