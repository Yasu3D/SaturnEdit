using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnData.Content.Cosmetics.NavigatorData;
using SaturnData.Utilities;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class NavigatorEditorView : UserControl
{
    public NavigatorEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;

#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxImageArtist.Text = navigator.Artist;
            TextBoxAudioArtist.Text = navigator.Voice;
            
            TextBoxWidth.Text = navigator.Size.Width.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxHeight.Text = navigator.Size.Height.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxOffsetX.Text = navigator.Offset.Horizontal.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxOffsetY.Text = navigator.Offset.Vertical.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginTop.Text = navigator.FaceMargin.Top.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginBottom.Text = navigator.FaceMargin.Bottom.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginLeft.Text = navigator.FaceMargin.Left.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginRight.Text = navigator.FaceMargin.Right.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxBlinkInterval.Text = (0.001f * navigator.BlinkAnimationInterval).ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxBlinkDuration.Text = (0.001f * navigator.BlinkAnimationDuration).ToString("0.000000", CultureInfo.InvariantCulture);
            
            TextBoxTextureIconPath.Text           = navigator.TexturePaths.GetValueOrDefault("icon",             "");
            TextBoxTextureBodyPath.Text           = navigator.TexturePaths.GetValueOrDefault("body",             "");
            TextBoxTextureFaceNeutralAPath.Text   = navigator.TexturePaths.GetValueOrDefault("face_neutral_a",   "");
            TextBoxTextureFaceNeutralBPath.Text   = navigator.TexturePaths.GetValueOrDefault("face_neutral_b",   "");
            TextBoxTextureFaceNeutralCPath.Text   = navigator.TexturePaths.GetValueOrDefault("face_neutral_c",   "");
            TextBoxTextureFaceAmazedAPath.Text    = navigator.TexturePaths.GetValueOrDefault("face_amazed_a",    "");
            TextBoxTextureFaceAmazedBPath.Text    = navigator.TexturePaths.GetValueOrDefault("face_amazed_b",    "");
            TextBoxTextureFaceAmazedCPath.Text    = navigator.TexturePaths.GetValueOrDefault("face_amazed_c",    "");
            TextBoxTextureFaceTroubledAPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_troubled_a",  "");
            TextBoxTextureFaceTroubledBPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_troubled_b",  "");
            TextBoxTextureFaceTroubledCPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_troubled_c",  "");
            TextBoxTextureFaceSurprisedAPath.Text = navigator.TexturePaths.GetValueOrDefault("face_surprised_a", "");
            TextBoxTextureFaceSurprisedBPath.Text = navigator.TexturePaths.GetValueOrDefault("face_surprised_b", "");
            TextBoxTextureFaceSurprisedCPath.Text = navigator.TexturePaths.GetValueOrDefault("face_surprised_c", "");
            TextBoxTextureFaceStartledAPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_startled_a",  "");
            TextBoxTextureFaceStartledBPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_startled_b",  "");
            TextBoxTextureFaceStartledCPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_startled_c",  "");
            TextBoxTextureFaceAngryAPath.Text     = navigator.TexturePaths.GetValueOrDefault("face_angry_a",     "");
            TextBoxTextureFaceAngryBPath.Text     = navigator.TexturePaths.GetValueOrDefault("face_angry_b",     "");
            TextBoxTextureFaceAngryCPath.Text     = navigator.TexturePaths.GetValueOrDefault("face_angry_c",     "");
            TextBoxTextureFaceLaughingAPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_laughing_a",  "");
            TextBoxTextureFaceLaughingBPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_laughing_b",  "");
            TextBoxTextureFaceLaughingCPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_laughing_c",  "");
            TextBoxTextureFaceSmilingAPath.Text   = navigator.TexturePaths.GetValueOrDefault("face_smiling_a",   "");
            TextBoxTextureFaceSmilingBPath.Text   = navigator.TexturePaths.GetValueOrDefault("face_smiling_b",   "");
            TextBoxTextureFaceSmilingCPath.Text   = navigator.TexturePaths.GetValueOrDefault("face_smiling_c",   "");
            TextBoxTextureFaceGrinningAPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_grinning_a",  "");
            TextBoxTextureFaceGrinningBPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_grinning_b",  "");
            TextBoxTextureFaceGrinningCPath.Text  = navigator.TexturePaths.GetValueOrDefault("face_grinning_c",  "");
            TextBoxTextureSeeYouAPath.Text        = navigator.TexturePaths.GetValueOrDefault("see_you_a",        "");
            TextBoxTextureSeeYouBPath.Text        = navigator.TexturePaths.GetValueOrDefault("see_you_b",        "");
            TextBoxTextureSeeYouCPath.Text        = navigator.TexturePaths.GetValueOrDefault("see_you_c",        "");
            
            IconFileNotFoundWarningIconPath.IsVisible           = TextBoxTextureIconPath.Text           != "" && !File.Exists(navigator.AbsoluteTexturePath("icon"));
            IconFileNotFoundWarningBodyPath.IsVisible           = TextBoxTextureBodyPath.Text           != "" && !File.Exists(navigator.AbsoluteTexturePath("body"));
            IconFileNotFoundWarningFaceNeutralAPath.IsVisible   = TextBoxTextureFaceNeutralAPath.Text   != "" && !File.Exists(navigator.AbsoluteTexturePath("face_neutral_a"));
            IconFileNotFoundWarningFaceNeutralBPath.IsVisible   = TextBoxTextureFaceNeutralBPath.Text   != "" && !File.Exists(navigator.AbsoluteTexturePath("face_neutral_b"));
            IconFileNotFoundWarningFaceNeutralCPath.IsVisible   = TextBoxTextureFaceNeutralCPath.Text   != "" && !File.Exists(navigator.AbsoluteTexturePath("face_neutral_c"));
            IconFileNotFoundWarningFaceAmazedAPath.IsVisible    = TextBoxTextureFaceAmazedAPath.Text    != "" && !File.Exists(navigator.AbsoluteTexturePath("face_amazed_a"));
            IconFileNotFoundWarningFaceAmazedBPath.IsVisible    = TextBoxTextureFaceAmazedBPath.Text    != "" && !File.Exists(navigator.AbsoluteTexturePath("face_amazed_b"));
            IconFileNotFoundWarningFaceAmazedCPath.IsVisible    = TextBoxTextureFaceAmazedCPath.Text    != "" && !File.Exists(navigator.AbsoluteTexturePath("face_amazed_c"));
            IconFileNotFoundWarningFaceTroubledAPath.IsVisible  = TextBoxTextureFaceTroubledAPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_troubled_a"));
            IconFileNotFoundWarningFaceTroubledBPath.IsVisible  = TextBoxTextureFaceTroubledBPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_troubled_b"));
            IconFileNotFoundWarningFaceTroubledCPath.IsVisible  = TextBoxTextureFaceTroubledCPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_troubled_c"));
            IconFileNotFoundWarningFaceSurprisedAPath.IsVisible = TextBoxTextureFaceSurprisedAPath.Text != "" && !File.Exists(navigator.AbsoluteTexturePath("face_surprised_a"));
            IconFileNotFoundWarningFaceSurprisedBPath.IsVisible = TextBoxTextureFaceSurprisedBPath.Text != "" && !File.Exists(navigator.AbsoluteTexturePath("face_surprised_b"));
            IconFileNotFoundWarningFaceSurprisedCPath.IsVisible = TextBoxTextureFaceSurprisedCPath.Text != "" && !File.Exists(navigator.AbsoluteTexturePath("face_surprised_c"));
            IconFileNotFoundWarningFaceStartledAPath.IsVisible  = TextBoxTextureFaceStartledAPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_startled_a"));
            IconFileNotFoundWarningFaceStartledBPath.IsVisible  = TextBoxTextureFaceStartledBPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_startled_b"));
            IconFileNotFoundWarningFaceStartledCPath.IsVisible  = TextBoxTextureFaceStartledCPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_startled_c"));
            IconFileNotFoundWarningFaceAngryAPath.IsVisible     = TextBoxTextureFaceAngryAPath.Text     != "" && !File.Exists(navigator.AbsoluteTexturePath("face_angry_a"));
            IconFileNotFoundWarningFaceAngryBPath.IsVisible     = TextBoxTextureFaceAngryBPath.Text     != "" && !File.Exists(navigator.AbsoluteTexturePath("face_angry_b"));
            IconFileNotFoundWarningFaceAngryCPath.IsVisible     = TextBoxTextureFaceAngryCPath.Text     != "" && !File.Exists(navigator.AbsoluteTexturePath("face_angry_c"));
            IconFileNotFoundWarningFaceLaughingAPath.IsVisible  = TextBoxTextureFaceLaughingAPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_laughing_a"));
            IconFileNotFoundWarningFaceLaughingBPath.IsVisible  = TextBoxTextureFaceLaughingBPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_laughing_b"));
            IconFileNotFoundWarningFaceLaughingCPath.IsVisible  = TextBoxTextureFaceLaughingCPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_laughing_c"));
            IconFileNotFoundWarningFaceSmilingAPath.IsVisible   = TextBoxTextureFaceSmilingAPath.Text   != "" && !File.Exists(navigator.AbsoluteTexturePath("face_smiling_a"));
            IconFileNotFoundWarningFaceSmilingBPath.IsVisible   = TextBoxTextureFaceSmilingBPath.Text   != "" && !File.Exists(navigator.AbsoluteTexturePath("face_smiling_b"));
            IconFileNotFoundWarningFaceSmilingCPath.IsVisible   = TextBoxTextureFaceSmilingCPath.Text   != "" && !File.Exists(navigator.AbsoluteTexturePath("face_smiling_c"));
            IconFileNotFoundWarningFaceGrinningAPath.IsVisible  = TextBoxTextureFaceGrinningAPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_grinning_a"));
            IconFileNotFoundWarningFaceGrinningBPath.IsVisible  = TextBoxTextureFaceGrinningBPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_grinning_b"));
            IconFileNotFoundWarningFaceGrinningCPath.IsVisible  = TextBoxTextureFaceGrinningCPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("face_grinning_c"));
            IconFileNotFoundWarningSeeYouAPath.IsVisible        = TextBoxTextureSeeYouAPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("see_you_a"));
            IconFileNotFoundWarningSeeYouBPath.IsVisible        = TextBoxTextureSeeYouBPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("see_you_b"));
            IconFileNotFoundWarningSeeYouCPath.IsVisible        = TextBoxTextureSeeYouCPath.Text  != "" && !File.Exists(navigator.AbsoluteTexturePath("see_you_c"));
            
            List<KeyValuePair<string, NavigatorLanguage>> languages = navigator.Writing.Languages.ToList();
            
            // Update existing language items
            for (int i = 0; i < languages.Count; i++)
            {
                KeyValuePair<string, NavigatorLanguage> language = languages[i];
                
                if (i < ListBoxLanguages.Items.Count)
                {
                    // Modify existing item.
                    if (ListBoxLanguages.Items[i] is not NavigatorLanguageItem item) continue;

                    item.SetItem(language.Value, language.Key);
                }
                else
                {
                    // Create new item.
                    NavigatorLanguageItem item = new();
                    item.SetItem(language.Value, language.Key);
                    
                    ListBoxLanguages.Items.Add(item);
                }
            }

            // Delete redundant language items.
            for (int i = ListBoxLanguages.Items.Count - 1; i >= languages.Count; i--)
            {
                if (ListBoxLanguages.Items[i] is not NavigatorLanguageItem item) continue;

                ListBoxLanguages.Items.Remove(item);
            }

            // Set language selection.
            ListBoxLanguages.SelectedItem = ListBoxLanguages.Items.FirstOrDefault(x =>
            {
                if (x is not NavigatorLanguageItem item) return false;
                if (item.NavigatorLanguage == null) return false;
                if (item.NavigatorLanguage != CosmeticSystem.SelectedNavigatorLanguage) return false;

                return true;
            });


            if (CosmeticSystem.SelectedNavigatorLanguage != null)
            {
                List<KeyValuePair<string, NavigatorDialogue>> lineCollections = CosmeticSystem.SelectedNavigatorLanguage.Dialogues.ToList();

                // Update existing dialogue items
                for (int i = 0; i < lineCollections.Count; i++)
                {
                    KeyValuePair<string, NavigatorDialogue> lineCollection = lineCollections[i];

                    if (i < ListBoxDialogues.Items.Count)
                    {
                        // Modify existing item.
                        if (ListBoxDialogues.Items[i] is not NavigatorDialogueItem item) continue;

                        item.SetItem(lineCollection.Value, lineCollection.Key);
                    }
                    else
                    {
                        // Create new item.
                        NavigatorDialogueItem item = new();
                        item.SetItem(lineCollection.Value, lineCollection.Key);

                        ListBoxDialogues.Items.Add(item);
                    }
                }

                // Delete redundant dialogue items.
                for (int i = ListBoxDialogues.Items.Count - 1; i >= lineCollections.Count; i--)
                {
                    if (ListBoxDialogues.Items[i] is not NavigatorDialogueItem item) continue;

                    ListBoxDialogues.Items.Remove(item);
                }

                // Set dialogue selection.
                ListBoxDialogues.SelectedItem = ListBoxDialogues.Items.FirstOrDefault(x =>
                {
                    if (x is not NavigatorDialogueItem item) return false;
                    if (item.NavigatorDialogue == null) return false;
                    if (item.NavigatorDialogue != CosmeticSystem.SelectedNavigatorDialogue) return false;

                    return true;
                });
            }
            else
            {
                ListBoxDialogues.Items.Clear();
            }


            if (CosmeticSystem.SelectedNavigatorDialogue != null)
            {
                List<NavigatorLine> lines = CosmeticSystem.SelectedNavigatorDialogue.Lines;

                // Update existing dialogue items
                for (int i = 0; i < lines.Count; i++)
                {
                    NavigatorLine line = lines[i];

                    if (i < ListBoxLines.Items.Count)
                    {
                        // Modify existing item.
                        if (ListBoxLines.Items[i] is not NavigatorLineItem item) continue;

                        item.SetItem(line);
                    }
                    else
                    {
                        // Create new item.
                        NavigatorLineItem item = new();
                        item.SetItem(line);

                        ListBoxLines.Items.Add(item);
                    }
                }

                // Delete redundant dialogue items.
                for (int i = ListBoxLines.Items.Count - 1; i >= lines.Count; i--)
                {
                    if (ListBoxLines.Items[i] is not NavigatorLineItem item) continue;

                    ListBoxLines.Items.Remove(item);
                }

                // Set dialogue selection.
                ListBoxLines.SelectedItem = ListBoxLines.Items.FirstOrDefault(x =>
                {
                    if (x is not NavigatorLineItem item) return false;
                    if (item.NavigatorLine == null) return false;
                    if (item.NavigatorLine != CosmeticSystem.SelectedNavigatorLine) return false;

                    return true;
                });
            }
            else
            {
                ListBoxLines.Items.Clear();
            }

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
    
    private void TextBoxImageArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
        
        string oldValue = navigator.Artist;
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { navigator.Artist = value; }, oldValue, newValue));
    }
    
    private void TextBoxAudioArtist_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
        
        string oldValue = navigator.Voice;
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { navigator.Voice = value; }, oldValue, newValue));
    }
    
    private void TextBoxWidth_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.Size.Width;
            float newValue = Convert.ToSingle(TextBoxWidth.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Size.Width = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Size.Width = value; }, navigator.Size.Width, 1024));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxHeight_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.Size.Height;
            float newValue = Convert.ToSingle(TextBoxHeight.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Size.Height = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Size.Height = value; }, navigator.Size.Height, 1024));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxOffsetX_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.Offset.Horizontal;
            float newValue = Convert.ToSingle(TextBoxOffsetX.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Offset.Horizontal = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Offset.Horizontal = value; }, navigator.Offset.Horizontal, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxOffsetY_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.Offset.Vertical;
            float newValue = Convert.ToSingle(TextBoxOffsetY.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Offset.Vertical = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.Offset.Vertical = value; }, navigator.Offset.Vertical, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxFaceMarginTop_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMargin.Top;
            float newValue = Convert.ToSingle(TextBoxFaceMarginTop.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Top = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Top = value; }, navigator.FaceMargin.Top, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxFaceMarginBottom_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMargin.Bottom;
            float newValue = Convert.ToSingle(TextBoxFaceMarginBottom.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Bottom = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Bottom = value; }, navigator.FaceMargin.Bottom, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxFaceMarginLeft_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMargin.Left;
            float newValue = Convert.ToSingle(TextBoxFaceMarginLeft.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Left = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Left = value; }, navigator.FaceMargin.Left, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    private void TextBoxFaceMarginRight_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMargin.Right;
            float newValue = Convert.ToSingle(TextBoxFaceMarginRight.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Right = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.FaceMargin.Right = value; }, navigator.FaceMargin.Right, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    
    private void TextBoxBlinkInterval_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.BlinkAnimationInterval;
            float newValue = 1000 * Convert.ToSingle(TextBoxBlinkInterval.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.BlinkAnimationInterval = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.BlinkAnimationInterval = value; }, navigator.BlinkAnimationInterval, 5000));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }

    private void TextBoxBlinkDuration_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.BlinkAnimationDuration;
            float newValue = 1000 * Convert.ToSingle(TextBoxBlinkDuration.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.BlinkAnimationDuration = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<float>(value => { navigator.BlinkAnimationDuration = value; }, navigator.BlinkAnimationDuration, 5000));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
    
    
    private void TextBoxTexturePath_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not TextBox textBox) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
        
        string oldValue = "";
        if      (textBox == TextBoxTextureIconPath)           { oldValue = navigator.TexturePaths.GetValueOrDefault("icon",             ""); }
        else if (textBox == TextBoxTextureBodyPath)           { oldValue = navigator.TexturePaths.GetValueOrDefault("body",             ""); }
        else if (textBox == TextBoxTextureFaceNeutralAPath)   { oldValue = navigator.TexturePaths.GetValueOrDefault("face_neutral_a",   ""); }
        else if (textBox == TextBoxTextureFaceNeutralBPath)   { oldValue = navigator.TexturePaths.GetValueOrDefault("face_neutral_b",   ""); }
        else if (textBox == TextBoxTextureFaceNeutralCPath)   { oldValue = navigator.TexturePaths.GetValueOrDefault("face_neutral_c",   ""); }
        else if (textBox == TextBoxTextureFaceAmazedAPath)    { oldValue = navigator.TexturePaths.GetValueOrDefault("face_amazed_a",    ""); }
        else if (textBox == TextBoxTextureFaceAmazedBPath)    { oldValue = navigator.TexturePaths.GetValueOrDefault("face_amazed_b",    ""); }
        else if (textBox == TextBoxTextureFaceAmazedCPath)    { oldValue = navigator.TexturePaths.GetValueOrDefault("face_amazed_c",    ""); }
        else if (textBox == TextBoxTextureFaceTroubledAPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_troubled_a",  ""); }
        else if (textBox == TextBoxTextureFaceTroubledBPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_troubled_b",  ""); }
        else if (textBox == TextBoxTextureFaceTroubledCPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_troubled_c",  ""); }
        else if (textBox == TextBoxTextureFaceSurprisedAPath) { oldValue = navigator.TexturePaths.GetValueOrDefault("face_surprised_a", ""); }
        else if (textBox == TextBoxTextureFaceSurprisedBPath) { oldValue = navigator.TexturePaths.GetValueOrDefault("face_surprised_b", ""); }
        else if (textBox == TextBoxTextureFaceSurprisedCPath) { oldValue = navigator.TexturePaths.GetValueOrDefault("face_surprised_c", ""); }
        else if (textBox == TextBoxTextureFaceStartledAPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_startled_a",  ""); }
        else if (textBox == TextBoxTextureFaceStartledBPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_startled_b",  ""); }
        else if (textBox == TextBoxTextureFaceStartledCPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_startled_c",  ""); }
        else if (textBox == TextBoxTextureFaceAngryAPath)     { oldValue = navigator.TexturePaths.GetValueOrDefault("face_angry_a",     ""); }
        else if (textBox == TextBoxTextureFaceAngryBPath)     { oldValue = navigator.TexturePaths.GetValueOrDefault("face_angry_b",     ""); }
        else if (textBox == TextBoxTextureFaceAngryCPath)     { oldValue = navigator.TexturePaths.GetValueOrDefault("face_angry_c",     ""); }
        else if (textBox == TextBoxTextureFaceLaughingAPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_laughing_a",  ""); }
        else if (textBox == TextBoxTextureFaceLaughingBPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_laughing_b",  ""); }
        else if (textBox == TextBoxTextureFaceLaughingCPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_laughing_c",  ""); }
        else if (textBox == TextBoxTextureFaceSmilingAPath)   { oldValue = navigator.TexturePaths.GetValueOrDefault("face_smiling_a",   ""); }
        else if (textBox == TextBoxTextureFaceSmilingBPath)   { oldValue = navigator.TexturePaths.GetValueOrDefault("face_smiling_b",   ""); }
        else if (textBox == TextBoxTextureFaceSmilingCPath)   { oldValue = navigator.TexturePaths.GetValueOrDefault("face_smiling_c",   ""); }
        else if (textBox == TextBoxTextureFaceGrinningAPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_grinning_a",  ""); }
        else if (textBox == TextBoxTextureFaceGrinningBPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_grinning_b",  ""); }
        else if (textBox == TextBoxTextureFaceGrinningCPath)  { oldValue = navigator.TexturePaths.GetValueOrDefault("face_grinning_c",  ""); }
        else if (textBox == TextBoxTextureSeeYouAPath)        { oldValue = navigator.TexturePaths.GetValueOrDefault("see_you_a",        ""); }
        else if (textBox == TextBoxTextureSeeYouBPath)        { oldValue = navigator.TexturePaths.GetValueOrDefault("see_you_b",        ""); }
        else if (textBox == TextBoxTextureSeeYouCPath)        { oldValue = navigator.TexturePaths.GetValueOrDefault("see_you_c",        ""); }
        
        string newValue = textBox.Text ?? "";
        if (oldValue == newValue)
        {
            // Refresh UI in case the file changed, but don't push unnecessary operation.
            CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
            return;
        }

        Action<string>? action = null;
        if      (textBox == TextBoxTextureIconPath)           { action = value => { navigator.TexturePaths["icon"] = value; }; }
        else if (textBox == TextBoxTextureBodyPath)           { action = value => { navigator.TexturePaths["body"] = value; }; }
        else if (textBox == TextBoxTextureFaceNeutralAPath)   { action = value => { navigator.TexturePaths["face_neutral_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceNeutralBPath)   { action = value => { navigator.TexturePaths["face_neutral_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceNeutralCPath)   { action = value => { navigator.TexturePaths["face_neutral_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceAmazedAPath)    { action = value => { navigator.TexturePaths["face_amazed_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceAmazedBPath)    { action = value => { navigator.TexturePaths["face_amazed_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceAmazedCPath)    { action = value => { navigator.TexturePaths["face_amazed_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceTroubledAPath)  { action = value => { navigator.TexturePaths["face_troubled_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceTroubledBPath)  { action = value => { navigator.TexturePaths["face_troubled_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceTroubledCPath)  { action = value => { navigator.TexturePaths["face_troubled_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceSurprisedAPath) { action = value => { navigator.TexturePaths["face_surprised_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceSurprisedBPath) { action = value => { navigator.TexturePaths["face_surprised_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceSurprisedCPath) { action = value => { navigator.TexturePaths["face_surprised_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceStartledAPath)  { action = value => { navigator.TexturePaths["face_startled_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceStartledBPath)  { action = value => { navigator.TexturePaths["face_startled_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceStartledCPath)  { action = value => { navigator.TexturePaths["face_startled_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceAngryAPath)     { action = value => { navigator.TexturePaths["face_angry_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceAngryBPath)     { action = value => { navigator.TexturePaths["face_angry_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceAngryCPath)     { action = value => { navigator.TexturePaths["face_angry_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceLaughingAPath)  { action = value => { navigator.TexturePaths["face_laughing_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceLaughingBPath)  { action = value => { navigator.TexturePaths["face_laughing_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceLaughingCPath)  { action = value => { navigator.TexturePaths["face_laughing_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceSmilingAPath)   { action = value => { navigator.TexturePaths["face_smiling_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceSmilingBPath)   { action = value => { navigator.TexturePaths["face_smiling_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceSmilingCPath)   { action = value => { navigator.TexturePaths["face_smiling_c"] = value; }; }
        else if (textBox == TextBoxTextureFaceGrinningAPath)  { action = value => { navigator.TexturePaths["face_grinning_a"] = value; }; }
        else if (textBox == TextBoxTextureFaceGrinningBPath)  { action = value => { navigator.TexturePaths["face_grinning_b"] = value; }; }
        else if (textBox == TextBoxTextureFaceGrinningCPath)  { action = value => { navigator.TexturePaths["face_grinning_c"] = value; }; }
        else if (textBox == TextBoxTextureSeeYouAPath)        { action = value => { navigator.TexturePaths["see_you_a"] = value; }; }
        else if (textBox == TextBoxTextureSeeYouBPath)        { action = value => { navigator.TexturePaths["see_you_b"] = value; }; }
        else if (textBox == TextBoxTextureSeeYouCPath)        { action = value => { navigator.TexturePaths["see_you_c"] = value; }; }

        if (action == null) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldValue, newValue));
    }

    private async void ButtonPickTextureFile_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (blockEvents) return;
            if (sender is not Button button) return;
            if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;
            
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            string? key = null;
            if      (button == ButtonTextureIconPath)           { key = "icon"; }
            else if (button == ButtonTextureBodyPath)           { key = "body"; }
            else if (button == ButtonTextureFaceNeutralAPath)   { key = "face_neutral_a"; }
            else if (button == ButtonTextureFaceNeutralBPath)   { key = "face_neutral_b"; }
            else if (button == ButtonTextureFaceNeutralCPath)   { key = "face_neutral_c"; }
            else if (button == ButtonTextureFaceAmazedAPath)    { key = "face_amazed_a"; }
            else if (button == ButtonTextureFaceAmazedBPath)    { key = "face_amazed_b"; }
            else if (button == ButtonTextureFaceAmazedCPath)    { key = "face_amazed_c"; }
            else if (button == ButtonTextureFaceTroubledAPath)  { key = "face_troubled_a"; }
            else if (button == ButtonTextureFaceTroubledBPath)  { key = "face_troubled_b"; }
            else if (button == ButtonTextureFaceTroubledCPath)  { key = "face_troubled_c"; }
            else if (button == ButtonTextureFaceSurprisedAPath) { key = "face_surprised_a"; }
            else if (button == ButtonTextureFaceSurprisedBPath) { key = "face_surprised_b"; }
            else if (button == ButtonTextureFaceSurprisedCPath) { key = "face_surprised_c"; }
            else if (button == ButtonTextureFaceStartledAPath)  { key = "face_startled_a"; }
            else if (button == ButtonTextureFaceStartledBPath)  { key = "face_startled_b"; }
            else if (button == ButtonTextureFaceStartledCPath)  { key = "face_startled_c"; }
            else if (button == ButtonTextureFaceAngryAPath)     { key = "face_angry_a"; }
            else if (button == ButtonTextureFaceAngryBPath)     { key = "face_angry_b"; }
            else if (button == ButtonTextureFaceAngryCPath)     { key = "face_angry_c"; }
            else if (button == ButtonTextureFaceLaughingAPath)  { key = "face_laughing_a"; }
            else if (button == ButtonTextureFaceLaughingBPath)  { key = "face_laughing_b"; }
            else if (button == ButtonTextureFaceLaughingCPath)  { key = "face_laughing_c"; }
            else if (button == ButtonTextureFaceSmilingAPath)   { key = "face_smiling_a"; }
            else if (button == ButtonTextureFaceSmilingBPath)   { key = "face_smiling_b"; }
            else if (button == ButtonTextureFaceSmilingCPath)   { key = "face_smiling_c"; }
            else if (button == ButtonTextureFaceGrinningAPath)  { key = "face_grinning_a"; }
            else if (button == ButtonTextureFaceGrinningBPath)  { key = "face_grinning_b"; }
            else if (button == ButtonTextureFaceGrinningCPath)  { key = "face_grinning_c"; }
            else if (button == ButtonTextureSeeYouAPath)        { key = "see_you_a"; }
            else if (button == ButtonTextureSeeYouBPath)        { key = "see_you_b"; }
            else if (button == ButtonTextureSeeYouCPath)        { key = "see_you_c"; }
            
            if (key == null) return;
            
            string oldLocalPath = navigator.TexturePaths.GetValueOrDefault(key, "");
            string oldAbsolutePath = navigator.AbsoluteTexturePath(key);
            
            // Open file picker.
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new("Image Files")
                    {
                        Patterns = ["*.png", "*.jpeg", "*.jpg"],
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
            
            Action<string> action = value => { navigator.TexturePaths[key] = value; };
            
            if (navigator.AbsoluteSourcePath == "")
            {
                // Define new source path.
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", $"navigator{SaturnFileExtensionList.SaturnContentFile}");
                
                GenericEditOperation<string> op0 = new(value => { navigator.AbsoluteSourcePath = value; }, navigator.AbsoluteSourcePath, newSourcePath);
                GenericEditOperation<string> op1 = new(action, oldLocalPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string directoryPath = Path.GetDirectoryName(navigator.AbsoluteSourcePath) ?? "";
                string localPath = Path.GetRelativePath(directoryPath, files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(directoryPath, localPath);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(action, oldLocalPath, localPath));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            LoggingSystem.WriteSessionLog(ex.ToString());
        }
    }


    private void ButtonDeleteLanguage_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLanguages?.SelectedItem is not NavigatorLanguageItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorLanguage == null) return;
        
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        DictionaryRemoveOperation<string, NavigatorLanguage> op0 = new(() => navigator.Writing.Languages, item.Key, item.NavigatorLanguage);
        GenericEditOperation<NavigatorLanguage?> op1 = new(value => { CosmeticSystem.SelectedNavigatorLanguage = value; }, CosmeticSystem.SelectedNavigatorLanguage, null);
        GenericEditOperation<NavigatorDialogue?> op2 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, null);
        GenericEditOperation<NavigatorLine?> op3 = new(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, null);

        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2, op3]));
    }

    private void ButtonAddLanguage_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        int c = 1;
        while (navigator.Writing.Languages.ContainsKey($"xx-XX {c.ToString(CultureInfo.InvariantCulture)}"))
        {
            c++;
        }
        
        string key = $"xx-XX {c}";
        NavigatorLanguage language = new();

        DictionaryAddOperation<string, NavigatorLanguage> op0 = new(() => navigator.Writing.Languages, key, language);
        GenericEditOperation<NavigatorLanguage?> op1 = new(value => { CosmeticSystem.SelectedNavigatorLanguage = value; }, CosmeticSystem.SelectedNavigatorLanguage, language);
        GenericEditOperation<NavigatorDialogue?> op2 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, null);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2]));
    }
    
    private void ListBoxLanguages_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLanguages?.SelectedItem is not NavigatorLanguageItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorLanguage == null) return;

        if (CosmeticSystem.SelectedNavigatorLanguage == item.NavigatorLanguage) return;
        
        NavigatorDialogue? newSelectedDialogue = item.NavigatorLanguage.Dialogues.FirstOrDefault().Value;
        NavigatorLine? newSelectedLine = newSelectedDialogue?.Lines.FirstOrDefault();
        
        GenericEditOperation<NavigatorLanguage?> op0 = new(value => { CosmeticSystem.SelectedNavigatorLanguage = value; }, CosmeticSystem.SelectedNavigatorLanguage, item.NavigatorLanguage);
        GenericEditOperation<NavigatorDialogue?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, newSelectedDialogue);
        GenericEditOperation<NavigatorLine?> op2 = new(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, newSelectedLine);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2]));
    }
    
    
    private void ButtonDeleteDialogue_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxDialogues?.SelectedItem is not NavigatorDialogueItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorDialogue == null) return;
        
        if (CosmeticSystem.SelectedNavigatorLanguage == null) return;

        DictionaryRemoveOperation<string, NavigatorDialogue> op0 = new(() => CosmeticSystem.SelectedNavigatorLanguage.Dialogues, item.Key, item.NavigatorDialogue);
        GenericEditOperation<NavigatorDialogue?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, null);
        GenericEditOperation<NavigatorLine?> op2 = new(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, null);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2]));
    }

    private void ButtonAddDialogue_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.SelectedNavigatorLanguage == null) return;
        
        int c = 1;
        while (CosmeticSystem.SelectedNavigatorLanguage.Dialogues.ContainsKey($"new_dialogue {c.ToString(CultureInfo.InvariantCulture)}"))
        {
            c++;
        }
        
        string key = $"new_dialogue {c}";
        NavigatorDialogue dialogue = new();

        DictionaryAddOperation<string, NavigatorDialogue> op0 = new(() => CosmeticSystem.SelectedNavigatorLanguage.Dialogues, key, dialogue);
        GenericEditOperation<NavigatorDialogue?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, dialogue);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    private void ListBoxDialogues_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxDialogues?.SelectedItem is not NavigatorDialogueItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorDialogue == null) return;
        
        if (CosmeticSystem.SelectedNavigatorLanguage == null) return;
        if (CosmeticSystem.SelectedNavigatorDialogue== item.NavigatorDialogue) return;
        
        NavigatorLine? newSelectedLine = item.NavigatorDialogue.Lines.FirstOrDefault();
        
        GenericEditOperation<NavigatorDialogue?> op0 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, item.NavigatorDialogue);
        GenericEditOperation<NavigatorLine?> op1 = new(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, newSelectedLine);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    
    private void ButtonDeleteLine_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLines?.SelectedItem is not NavigatorLineItem item) return;
        if (item.NavigatorLine == null) return;

        if (CosmeticSystem.SelectedNavigatorDialogue == null) return;

        ListRemoveOperation<NavigatorLine> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogue.Lines, item.NavigatorLine);
        GenericEditOperation<NavigatorLine?> op1 = new(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, null);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }

    private void ButtonAddLine_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.SelectedNavigatorDialogue == null) return;

        NavigatorLine line = new();

        ListAddOperation<NavigatorLine> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogue.Lines, line);
        GenericEditOperation<NavigatorLine?> op1 = new(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, line);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    private void ListBoxLines_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLines?.SelectedItem is not NavigatorLineItem item) return;
        if (CosmeticSystem.SelectedNavigatorDialogue == null) return;
        if (item.NavigatorLine == null) return;

        if (CosmeticSystem.SelectedNavigatorLine == item.NavigatorLine) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<NavigatorLine?>(value => { CosmeticSystem.SelectedNavigatorLine = value; }, CosmeticSystem.SelectedNavigatorLine, item.NavigatorLine));
    }
#endregion UI Event Handlers
}