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
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.PrimitiveOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class NavigatorEditorView : UserControl
{
    public NavigatorEditorView()
    {
        InitializeComponent();
        
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
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
            
            TextBoxWidth.Text = navigator.Width.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxHeight.Text = navigator.Height.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxOffsetX.Text = navigator.OffsetX.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxOffsetY.Text = navigator.OffsetY.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginTop.Text = navigator.FaceMarginTop.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginBottom.Text = navigator.FaceMarginBottom.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginLeft.Text = navigator.FaceMarginLeft.ToString("0.000000", CultureInfo.InvariantCulture);
            TextBoxFaceMarginRight.Text = navigator.FaceMarginRight.ToString("0.000000", CultureInfo.InvariantCulture);
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
            
            List<KeyValuePair<string, NavigatorDialogueLanguage>> languages = navigator.DialogueLanguages.ToList();
            
            // Update existing language items
            for (int i = 0; i < languages.Count; i++)
            {
                KeyValuePair<string, NavigatorDialogueLanguage> language = languages[i];
                
                if (i < ListBoxLanguages.Items.Count)
                {
                    // Modify existing item.
                    if (ListBoxLanguages.Items[i] is not NavigatorDialogueLanguageItem item) continue;

                    item.SetItem(language.Value, language.Key);
                }
                else
                {
                    // Create new item.
                    NavigatorDialogueLanguageItem item = new();
                    item.SetItem(language.Value, language.Key);
                    
                    ListBoxLanguages.Items.Add(item);
                }
            }

            // Delete redundant language items.
            for (int i = ListBoxLanguages.Items.Count - 1; i >= languages.Count; i--)
            {
                if (ListBoxLanguages.Items[i] is not NavigatorDialogueLanguageItem item) continue;

                ListBoxLanguages.Items.Remove(item);
            }

            // Set language selection.
            ListBoxLanguages.SelectedItem = ListBoxLanguages.Items.FirstOrDefault(x =>
            {
                if (x is not NavigatorDialogueLanguageItem item) return false;
                if (item.NavigatorDialogueLanguage == null) return false;
                if (item.NavigatorDialogueLanguage != CosmeticSystem.SelectedNavigatorDialogueLanguage) return false;

                return true;
            });


            if (CosmeticSystem.SelectedNavigatorDialogueLanguage != null)
            {
                List<KeyValuePair<string, NavigatorDialogueVariantCollection>> variantCollections = CosmeticSystem.SelectedNavigatorDialogueLanguage.Dialogues.ToList();

                // Update existing dialogue items
                for (int i = 0; i < variantCollections.Count; i++)
                {
                    KeyValuePair<string, NavigatorDialogueVariantCollection> variantCollection = variantCollections[i];

                    if (i < ListBoxDialogues.Items.Count)
                    {
                        // Modify existing item.
                        if (ListBoxDialogues.Items[i] is not NavigatorDialogueVariantCollectionItem item) continue;

                        item.SetItem(variantCollection.Value, variantCollection.Key);
                    }
                    else
                    {
                        // Create new item.
                        NavigatorDialogueVariantCollectionItem item = new();
                        item.SetItem(variantCollection.Value, variantCollection.Key);

                        ListBoxDialogues.Items.Add(item);
                    }
                }

                // Delete redundant dialogue items.
                for (int i = ListBoxDialogues.Items.Count - 1; i >= variantCollections.Count; i--)
                {
                    if (ListBoxDialogues.Items[i] is not NavigatorDialogueVariantCollectionItem item) continue;

                    ListBoxDialogues.Items.Remove(item);
                }

                // Set dialogue selection.
                ListBoxDialogues.SelectedItem = ListBoxDialogues.Items.FirstOrDefault(x =>
                {
                    if (x is not NavigatorDialogueVariantCollectionItem item) return false;
                    if (item.NavigatorDialogueVariantCollection == null) return false;
                    if (item.NavigatorDialogueVariantCollection != CosmeticSystem.SelectedNavigatorDialogueVariantCollection) return false;

                    return true;
                });
            }
            else
            {
                ListBoxDialogues.Items.Clear();
            }


            if (CosmeticSystem.SelectedNavigatorDialogueVariantCollection != null)
            {
                List<NavigatorDialogue> variants = CosmeticSystem.SelectedNavigatorDialogueVariantCollection.Variants;

                // Update existing dialogue items
                for (int i = 0; i < variants.Count; i++)
                {
                    NavigatorDialogue variant = variants[i];

                    if (i < ListBoxVariants.Items.Count)
                    {
                        // Modify existing item.
                        if (ListBoxVariants.Items[i] is not NavigatorDialogueItem item) continue;

                        item.SetItem(variant);
                    }
                    else
                    {
                        // Create new item.
                        NavigatorDialogueItem item = new();
                        item.SetItem(variant);

                        ListBoxVariants.Items.Add(item);
                    }
                }

                // Delete redundant dialogue items.
                for (int i = ListBoxVariants.Items.Count - 1; i >= variants.Count; i--)
                {
                    if (ListBoxVariants.Items[i] is not NavigatorDialogueItem item) continue;

                    ListBoxVariants.Items.Remove(item);
                }

                // Set dialogue selection.
                ListBoxVariants.SelectedItem = ListBoxVariants.Items.FirstOrDefault(x =>
                {
                    if (x is not NavigatorDialogueItem item) return false;
                    if (item.NavigatorDialogue == null) return false;
                    if (item.NavigatorDialogue != CosmeticSystem.SelectedNavigatorDialogue) return false;

                    return true;
                });
            }
            else
            {
                ListBoxVariants.Items.Clear();
            }

            blockEvents = false;
        });
    }
#endregion System Event Handlers
    
#region UI Event Handlers
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
        
        UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(value => { navigator.Artist = value; }, oldValue, newValue));
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
        
        UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(value => { navigator.Voice = value; }, oldValue, newValue));
    }
    
    private void TextBoxWidth_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.Width;
            float newValue = Convert.ToSingle(TextBoxWidth.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.Width = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.Width = value; }, navigator.Width, 1024));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxHeight_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.Height;
            float newValue = Convert.ToSingle(TextBoxHeight.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.Height = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.Height = value; }, navigator.Height, 1024));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxOffsetX_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.OffsetX;
            float newValue = Convert.ToSingle(TextBoxOffsetX.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.OffsetX = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.OffsetX = value; }, navigator.OffsetX, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxOffsetY_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.OffsetY;
            float newValue = Convert.ToSingle(TextBoxOffsetY.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.OffsetY = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.OffsetY = value; }, navigator.OffsetY, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxFaceMarginTop_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMarginTop;
            float newValue = Convert.ToSingle(TextBoxFaceMarginTop.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginTop = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginTop = value; }, navigator.FaceMarginTop, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxFaceMarginBottom_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMarginBottom;
            float newValue = Convert.ToSingle(TextBoxFaceMarginBottom.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginBottom = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginBottom = value; }, navigator.FaceMarginBottom, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxFaceMarginLeft_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMarginLeft;
            float newValue = Convert.ToSingle(TextBoxFaceMarginLeft.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginLeft = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginLeft = value; }, navigator.FaceMarginLeft, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
    
    private void TextBoxFaceMarginRight_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        try
        {
            float oldValue = navigator.FaceMarginRight;
            float newValue = Convert.ToSingle(TextBoxFaceMarginRight.Text ?? "", CultureInfo.InvariantCulture);
            if (oldValue == newValue) return;

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginRight = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.FaceMarginRight = value; }, navigator.FaceMarginRight, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
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

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.BlinkAnimationInterval = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.BlinkAnimationInterval = value; }, navigator.BlinkAnimationInterval, 5000));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
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

            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.BlinkAnimationDuration = value; }, oldValue, newValue));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new FloatEditOperation(value => { navigator.BlinkAnimationDuration = value; }, navigator.BlinkAnimationDuration, 5000));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
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
        
        UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(action, oldValue, newValue));
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
                string newSourcePath = Path.Combine(Path.GetDirectoryName(files[0].Path.LocalPath) ?? "", "navigator.toml");
                
                StringEditOperation op0 = new(value => { navigator.AbsoluteSourcePath = value; }, navigator.AbsoluteSourcePath, newSourcePath);
                StringEditOperation op1 = new(action, oldLocalPath, Path.GetFileName(files[0].Path.LocalPath));
                
                UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
            }
            else
            {
                // Use existing root directory.
                string filename = Path.GetFileName(files[0].Path.LocalPath);
                string pathFromRootDirectory = Path.Combine(Path.GetDirectoryName(navigator.AbsoluteSourcePath) ?? "", filename);

                // Prompt user to move or copy the selected file if it's not in the root directory yet.
                if (!await MainWindow.PromptFileMoveAndOverwrite(files[0].Path.LocalPath, pathFromRootDirectory)) return;

                UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(action, oldLocalPath, filename));
            }
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }


    private void ButtonDeleteLanguage_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLanguages?.SelectedItem is not NavigatorDialogueLanguageItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorDialogueLanguage == null) return;
        
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        DictionaryRemoveOperation<string, NavigatorDialogueLanguage> op0 = new(() => navigator.DialogueLanguages, item.Key, item.NavigatorDialogueLanguage);
        GenericEditOperation<NavigatorDialogueLanguage?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogueLanguage = value; }, CosmeticSystem.SelectedNavigatorDialogueLanguage, null);
        GenericEditOperation<NavigatorDialogueVariantCollection?> op2 = new(value => { CosmeticSystem.SelectedNavigatorDialogueVariantCollection = value; }, CosmeticSystem.SelectedNavigatorDialogueVariantCollection, null);
        GenericEditOperation<NavigatorDialogue?> op3 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, null);

        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2, op3]));
    }

    private void ButtonAddLanguage_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        int c = 1;
        while (navigator.DialogueLanguages.ContainsKey($"xx-XX {c.ToString(CultureInfo.InvariantCulture)}"))
        {
            c++;
        }
        
        string key = $"xx-XX {c}";
        NavigatorDialogueLanguage language = new();

        DictionaryAddOperation<string, NavigatorDialogueLanguage> op0 = new(() => navigator.DialogueLanguages, key, language);
        GenericEditOperation<NavigatorDialogueLanguage?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogueLanguage = value; }, CosmeticSystem.SelectedNavigatorDialogueLanguage, language);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    private void ListBoxLanguages_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLanguages?.SelectedItem is not NavigatorDialogueLanguageItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorDialogueLanguage == null) return;

        if (CosmeticSystem.SelectedNavigatorDialogueLanguage == item.NavigatorDialogueLanguage) return;
        
        NavigatorDialogueVariantCollection? newSelectedVariantCollection = item.NavigatorDialogueLanguage.Dialogues.FirstOrDefault().Value;
        NavigatorDialogue? newSelectedDialogue = newSelectedVariantCollection?.Variants.FirstOrDefault();
        
        GenericEditOperation<NavigatorDialogueLanguage?> op0 = new(value => { CosmeticSystem.SelectedNavigatorDialogueLanguage = value; }, CosmeticSystem.SelectedNavigatorDialogueLanguage, item.NavigatorDialogueLanguage);
        GenericEditOperation<NavigatorDialogueVariantCollection?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogueVariantCollection = value; }, CosmeticSystem.SelectedNavigatorDialogueVariantCollection, newSelectedVariantCollection);
        GenericEditOperation<NavigatorDialogue?> op2 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, newSelectedDialogue);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2]));
    }
    
    
    private void ButtonDeleteDialogue_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxDialogues?.SelectedItem is not NavigatorDialogueVariantCollectionItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorDialogueVariantCollection == null) return;
        
        if (CosmeticSystem.SelectedNavigatorDialogueLanguage == null) return;

        DictionaryRemoveOperation<string, NavigatorDialogueVariantCollection> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogueLanguage.Dialogues, item.Key, item.NavigatorDialogueVariantCollection);
        GenericEditOperation<NavigatorDialogueVariantCollection?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogueVariantCollection = value; }, CosmeticSystem.SelectedNavigatorDialogueVariantCollection, null);
        GenericEditOperation<NavigatorDialogue?> op2 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, null);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1, op2]));
    }

    private void ButtonAddDialogue_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.SelectedNavigatorDialogueLanguage == null) return;
        
        int c = 1;
        while (CosmeticSystem.SelectedNavigatorDialogueLanguage.Dialogues.ContainsKey($"new_dialogue {c.ToString(CultureInfo.InvariantCulture)}"))
        {
            c++;
        }
        
        string key = $"new_dialogue {c}";
        NavigatorDialogueVariantCollection dialogue = new();

        DictionaryAddOperation<string, NavigatorDialogueVariantCollection> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogueLanguage.Dialogues, key, dialogue);
        GenericEditOperation<NavigatorDialogueVariantCollection?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogueVariantCollection = value; }, CosmeticSystem.SelectedNavigatorDialogueVariantCollection, dialogue);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    private void ListBoxDialogues_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxDialogues?.SelectedItem is not NavigatorDialogueVariantCollectionItem item) return;
        if (item.Key == null) return;
        if (item.NavigatorDialogueVariantCollection == null) return;
        
        if (CosmeticSystem.SelectedNavigatorDialogueLanguage == null) return;
        if (CosmeticSystem.SelectedNavigatorDialogueVariantCollection== item.NavigatorDialogueVariantCollection) return;
        
        NavigatorDialogue? newSelectedDialogue = item.NavigatorDialogueVariantCollection.Variants.FirstOrDefault();
        
        GenericEditOperation<NavigatorDialogueVariantCollection?> op0 = new(value => { CosmeticSystem.SelectedNavigatorDialogueVariantCollection = value; }, CosmeticSystem.SelectedNavigatorDialogueVariantCollection, item.NavigatorDialogueVariantCollection);
        GenericEditOperation<NavigatorDialogue?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, newSelectedDialogue);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    
    private void ButtonDeleteVariant_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxVariants?.SelectedItem is not NavigatorDialogueItem item) return;
        if (item.NavigatorDialogue == null) return;

        if (CosmeticSystem.SelectedNavigatorDialogueVariantCollection == null) return;

        ListRemoveOperation<NavigatorDialogue> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogueVariantCollection.Variants, item.NavigatorDialogue);
        GenericEditOperation<NavigatorDialogue?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, null);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }

    private void ButtonAddVariant_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.SelectedNavigatorDialogueVariantCollection == null) return;

        NavigatorDialogue variant = new();

        ListAddOperation<NavigatorDialogue> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogueVariantCollection.Variants, variant);
        GenericEditOperation<NavigatorDialogue?> op1 = new(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, variant);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
    
    private void ListBoxVariants_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxVariants?.SelectedItem is not NavigatorDialogueItem item) return;
        if (CosmeticSystem.SelectedNavigatorDialogueVariantCollection == null) return;
        if (item.NavigatorDialogue == null) return;

        if (CosmeticSystem.SelectedNavigatorDialogue == item.NavigatorDialogue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<NavigatorDialogue?>(value => { CosmeticSystem.SelectedNavigatorDialogue = value; }, CosmeticSystem.SelectedNavigatorDialogue, item.NavigatorDialogue));
    }
#endregion UI Event Handlers
}