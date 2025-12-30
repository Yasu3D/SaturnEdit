using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using ConsoleColor = SaturnData.Content.Cosmetics.ConsoleColor;

namespace SaturnEdit.Windows.CosmeticsEditor;

public partial class CosmeticsEditorView : UserControl
{
    public CosmeticsEditorView()
    {
        InitializeComponent();
    }

    private static event EventHandler? NavigatorExpressionRefresh;
        
    // ReSharper disable once UnusedMember.Local
    private static Timer navigatorTimer = new(NavigatorTimer_OnTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(10));
    
    private static readonly Dictionary<string, Bitmap> NavigatorTextures = [];
    private static NavigatorExpression navigatorExpression = NavigatorExpression.Neutral;
    private static NavigatorBlinkState navigatorBlinkState = NavigatorBlinkState.Open;
    private static float navigatorTime = 0;
    
    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;
    
#region Methods
    private async void File_New(CosmeticType cosmeticType)
    {
        // Prompt to save an unsaved cosmetic first.
        if (!CosmeticSystem.IsSaved)
        {
            ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Cosmetic);

            // Cancel
            if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return;

            // Save
            if (result is ModalDialogResult.Primary)
            {
                bool success = await File_Save();

                // Abort opening new file if save was unsuccessful.
                if (!success) return;
            }

            // Don't Save
            // Continue as normal.
        }
        
        CosmeticSystem.NewCosmetic(cosmeticType);
    }
    
    private async Task<bool> File_Open(CosmeticType cosmeticType)
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;

            // Prompt to save an unsaved cosmetic first.
            if (!CosmeticSystem.IsSaved)
            {
                ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Cosmetic);

                // Cancel
                if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return false;

                // Save
                if (result is ModalDialogResult.Primary)
                {
                    bool success = await File_Save();

                    // Abort opening new file if save was unsuccessful.
                    if (!success) return false;
                }

                // Don't Save
                // Continue as normal.
            }

            // Open file picker.
            string fileTypeFilterName = cosmeticType switch
            {
                CosmeticType.ConsoleColor => "Console Color Files",
                CosmeticType.Emblem => "Emblem Files",
                CosmeticType.Icon => "Icon Files",
                CosmeticType.Navigator => "Navigator Files",
                CosmeticType.NoteSound => "Note Sound Files",
                CosmeticType.Plate => "Plate Files",
                CosmeticType.SystemMusic => "System Music Files",
                CosmeticType.SystemSound => "System Sound Files",
                CosmeticType.Title => "Title Files",
                _ => throw new ArgumentOutOfRangeException(nameof(cosmeticType), cosmeticType, null),
            };
            
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(new()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new(fileTypeFilterName)
                    {
                        Patterns = ["*.toml"],
                    },
                ],
            });
            if (files.Count != 1) return false;
            
            // Read cosmetic from file.
            CosmeticSystem.ReadCosmetic(files[0].Path.LocalPath, cosmeticType);
            return true;
        }
        catch (Exception ex)
        {
            LoggingSystem.WriteSessionLog(ex.ToString());
            return false;
        }
    }
    
    public async Task<bool> File_Save()
    {
        try
        {
            // Redirect to 'Save As' if cosmetic doesn't have a file defined yet.
            if (!File.Exists(CosmeticSystem.CosmeticItem.AbsoluteSourcePath))
            {
                return await File_SaveAs();
            }

            // Write cosmetic to file.
            if (!CosmeticSystem.WriteCosmetic(CosmeticSystem.CosmeticItem.AbsoluteSourcePath, true, false))
            {
                MainWindow.ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LoggingSystem.WriteSessionLog(ex.ToString());
            MainWindow.ShowFileWriteError();
            return false;
        }
    }
    
    private async Task<bool> File_SaveAs()
    {
        try
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return false;

            // Open file picker.
            string suggestedFileName = CosmeticSystem.CosmeticItem switch
            {
                ConsoleColor => "console_color.toml",
                Emblem => "emblem.toml",
                Icon => "icon.toml",
                Navigator => "navigator.toml",
                NoteSound => "note_sound.toml",
                Plate => "plate.toml",
                SystemMusic => "system_music.toml",
                SystemSound => "system_sound.toml",
                Title => "title.toml",
                _ => "cosmetic.toml",
            };
            
            string fileTypeChoiceName = CosmeticSystem.CosmeticItem switch
            {
                ConsoleColor => "Console Color File",
                Emblem => "Emblem File",
                Icon => "Icon File",
                Navigator => "Navigator File",
                NoteSound => "Note Sound File",
                Plate => "Plate File",
                SystemMusic => "System Music File",
                SystemSound => "System Sound File",
                Title => "Title File",
                _ => "Cosmetic File",
            };

            IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(new()
            {
                DefaultExtension = ".toml",
                SuggestedFileName = suggestedFileName,
                FileTypeChoices =
                [
                    new(fileTypeChoiceName)
                    {
                        Patterns = ["*.toml"],
                    },
                ],
            });

            if (file == null) return false;

            // Write cosmetic to file.
            if (!CosmeticSystem.WriteCosmetic(file.Path.LocalPath, true, true))
            {
                MainWindow.ShowFileWriteError();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            LoggingSystem.WriteSessionLog(ex.ToString());
            MainWindow.ShowFileWriteError();
            return false;
        }
    }
    
    private async Task<bool> File_ReloadFromDisk()
    {
        try
        {
            if (!File.Exists(CosmeticSystem.CosmeticItem.AbsoluteSourcePath)) return false;

            // Prompt to save an unsaved cosmetic first.
            if (!CosmeticSystem.IsSaved)
            {
                ModalDialogResult result = await MainWindow.ShowSavePrompt(SavePromptType.Cosmetic);

                // Cancel
                if (result is ModalDialogResult.Cancel or ModalDialogResult.Tertiary) return false;

                // Save
                if (result is ModalDialogResult.Primary)
                {
                    bool success = await File_Save();

                    // Abort opening new file if save was unsuccessful.
                    if (!success) return false;
                }

                // Don't Save
                // Continue as normal.
            }

            CosmeticType cosmeticType = CosmeticSystem.CosmeticItem switch
            {
                ConsoleColor => CosmeticType.ConsoleColor,
                Emblem => CosmeticType.Emblem,
                Icon => CosmeticType.Icon,
                Navigator => CosmeticType.Navigator,
                NoteSound => CosmeticType.NoteSound,
                Plate => CosmeticType.Plate,
                SystemMusic => CosmeticType.SystemMusic,
                SystemSound => CosmeticType.SystemSound,
                Title => CosmeticType.Title,
                _ => throw new ArgumentOutOfRangeException(),
            };
            
            CosmeticSystem.ReadCosmetic(CosmeticSystem.CosmeticItem.AbsoluteSourcePath, cosmeticType);
            return true;
        }
        catch (Exception ex)
        {
            LoggingSystem.WriteSessionLog(ex.ToString());
            return false;
        }
    }

    private void File_Quit()
    {
        if (VisualRoot is not Window rootWindow) return;
        rootWindow.Close();
    }

    private void UpdateNavigator()
    {
        if (CosmeticSystem.CosmeticItem is not Navigator navigator)
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupNavigator.IsVisible = false;
            });
            
            return;
        }
        
        NavigatorTextures.Clear();

        foreach (KeyValuePair<string, string> pair in navigator.TexturePaths)
        {
            string absolutePath = navigator.AbsoluteTexturePath(pair.Key);
            
            if (!File.Exists(absolutePath)) continue;
            
            try
            {
                NavigatorTextures[absolutePath] = new(absolutePath);
            }
            catch (Exception ex)
            {
                // Don't throw.
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
        
        Dispatcher.UIThread.Post(() =>
        {
            const float scale = 1;
         
            GroupNavigator.IsVisible = true;
            
            PanelNavigatorContainer.Width = navigator.Width * scale;
            PanelNavigatorContainer.Height = navigator.Height * scale;

            ImageNavigatorFace.Margin = new(navigator.FaceMarginLeft * scale, navigator.FaceMarginTop * scale, navigator.FaceMarginRight * scale, navigator.FaceMarginBottom * scale);
            ImageNavigatorBody.Source = NavigatorTextures.GetValueOrDefault(navigator.AbsoluteTexturePath("body"));
        });
    }
    
    private void UpdateNavigatorExpression()
    {
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        string expressionKey = navigatorExpression switch
        {
            NavigatorExpression.Neutral => "neutral",
            NavigatorExpression.Amazed => "amazed",
            NavigatorExpression.Troubled => "troubled",
            NavigatorExpression.Surprised => "surprised",
            NavigatorExpression.Startled => "startled",
            NavigatorExpression.Angry => "angry",
            NavigatorExpression.Laughing => "laughing",
            NavigatorExpression.Smiling => "smiling",
            NavigatorExpression.Grinning => "grinning",
            _ => throw new ArgumentOutOfRangeException(),
        };
            
        string blinkStateKey = navigatorBlinkState switch
        {
            NavigatorBlinkState.Open => "a",
            NavigatorBlinkState.HalfClose => "b",
            NavigatorBlinkState.Closed => "c",
            _ => throw new ArgumentOutOfRangeException(),
        };

        Bitmap? neutralA = NavigatorTextures.GetValueOrDefault(navigator.AbsoluteTexturePath("face_neutral_a"));
        Bitmap? neutralBlink = NavigatorTextures.GetValueOrDefault(navigator.AbsoluteTexturePath($"face_neutral_{blinkStateKey}"));
        Bitmap? expressionBlink = NavigatorTextures.GetValueOrDefault(navigator.AbsoluteTexturePath($"face_{expressionKey}_{blinkStateKey}"));

        Bitmap? final = expressionBlink ?? neutralBlink ?? neutralA;
        
        Dispatcher.UIThread.Post(() =>
        {
            ImageNavigatorFace.Source = final;
        });
    }

    private void UpdateProfile()
    {
        if (CosmeticSystem.CosmeticItem is Plate plate)
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupProfile.IsVisible = true;
                
                ImagePlaceholderIcon.IsVisible = true;
                ImageIcon.IsVisible = false;
                BorderIcon.IsVisible = false;

                ImagePlaceholderEmblem.IsVisible = true;
                ImageEmblem.IsVisible = false;
                BorderEmblem.IsVisible = false;

                BorderPlate.IsVisible = true;
                
                TextBlockTitle.Text = "TITLE";
                
                try
                {
                    ImagePlate.Source = new Bitmap(plate.AbsoluteImagePath);
                    
                    ImagePlaceholderPlate.IsVisible = false;
                    ImagePlate.IsVisible = true;
                }
                catch (Exception ex)
                {
                    // Don't throw.
                    if (ex is not (FileNotFoundException or ArgumentException))
                    {
                        LoggingSystem.WriteSessionLog(ex.ToString());
                    }
                    
                    ImagePlaceholderPlate.IsVisible = false;
                    ImagePlate.IsVisible = false;
                }
            });
        }
        else if (CosmeticSystem.CosmeticItem is Icon icon)
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupProfile.IsVisible = true;
                
                ImagePlaceholderPlate.IsVisible = true;
                ImagePlate.IsVisible = false;
                BorderPlate.IsVisible = false;

                ImagePlaceholderEmblem.IsVisible = true;
                ImageEmblem.IsVisible = false;
                BorderEmblem.IsVisible = false;
                
                BorderIcon.IsVisible = true;
                
                TextBlockTitle.Text = "TITLE";
                
                try
                {
                    ImageIcon.Source = new Bitmap(icon.AbsoluteImagePath);
                    
                    ImagePlaceholderIcon.IsVisible = false;
                    ImageIcon.IsVisible = true;
                }
                catch (Exception ex)
                {
                    // Don't throw.
                    if (ex is not (FileNotFoundException or ArgumentException))
                    {
                        LoggingSystem.WriteSessionLog(ex.ToString());
                    }
                    
                    ImagePlaceholderIcon.IsVisible = false;
                    ImageIcon.IsVisible = false;
                }
            });
        }
        else if (CosmeticSystem.CosmeticItem is Emblem emblem)
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupProfile.IsVisible = true;
                
                ImagePlaceholderPlate.IsVisible = true;
                ImagePlate.IsVisible = false;
                BorderPlate.IsVisible = false;
                
                ImagePlaceholderIcon.IsVisible = true;
                ImageIcon.IsVisible = false;
                BorderIcon.IsVisible = false;
                
                BorderEmblem.IsVisible = true;
                
                TextBlockTitle.Text = "TITLE";
                
                try
                {
                    ImageEmblem.Source = new Bitmap(emblem.AbsoluteImagePath);
                    
                    ImagePlaceholderEmblem.IsVisible = false;
                    ImageEmblem.IsVisible = true;
                }
                catch (Exception ex)
                {
                    // Don't throw.
                    if (ex is not (FileNotFoundException or ArgumentException))
                    {
                        LoggingSystem.WriteSessionLog(ex.ToString());
                    }
                    
                    ImagePlaceholderEmblem.IsVisible = false;
                    ImageEmblem.IsVisible = false;
                }
            });
        }
        else if (CosmeticSystem.CosmeticItem is Title title)
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupProfile.IsVisible = true;
                
                ImagePlaceholderPlate.IsVisible = true;
                ImagePlate.IsVisible = false;
                BorderPlate.IsVisible = false;
                
                ImagePlaceholderIcon.IsVisible = true;
                ImageIcon.IsVisible = false;
                BorderIcon.IsVisible = false;

                ImagePlaceholderEmblem.IsVisible = true;
                ImageEmblem.IsVisible = false;
                BorderEmblem.IsVisible = false;
                
                TextBlockTitle.Text = title.Message;
            });
        }
        else
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupProfile.IsVisible = false;
            });
        }
        
        
    }

    private void UpdateConsoleColor()
    {
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor)
        {
            Dispatcher.UIThread.Post(() =>
            {
                GroupConsoleColor.IsVisible = false;
            });

            return;
        }
        
        Dispatcher.UIThread.Post(() =>
        {
            GroupConsoleColor.IsVisible = true;

            PathIconConsoleColorA.Foreground = new SolidColorBrush(consoleColor.ColorA);
            PathIconConsoleColorB.Foreground = new SolidColorBrush(consoleColor.ColorB);
            PathIconConsoleColorC.Foreground = new SolidColorBrush(consoleColor.ColorC);
        });
    }
#endregion Methods
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemSave.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Save"].ToKeyGesture();
            MenuItemSaveAs.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"].ToKeyGesture();
            MenuItemReloadFromDisk.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"].ToKeyGesture();
            MenuItemSettings.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToKeyGesture();
            MenuItemQuit.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"].ToKeyGesture();

            MenuItemUndo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToKeyGesture();
            MenuItemRedo.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToKeyGesture();
        });
    }

    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            MenuItemReloadFromDisk.IsEnabled = File.Exists(CosmeticSystem.CosmeticItem.AbsoluteSourcePath);
            
            ConsoleColorEditor.IsVisible = CosmeticSystem.CosmeticItem is ConsoleColor;
            ConsoleColorEditor.IsEnabled = ConsoleColorEditor.IsVisible;
            
            EmblemEditor.IsVisible = CosmeticSystem.CosmeticItem is Emblem;
            EmblemEditor.IsEnabled = EmblemEditor.IsVisible;
            
            IconEditor.IsVisible = CosmeticSystem.CosmeticItem is Icon;
            IconEditor.IsEnabled = IconEditor.IsVisible;
            
            NavigatorEditor.IsVisible = CosmeticSystem.CosmeticItem is Navigator;
            NavigatorEditor.IsEnabled = NavigatorEditor.IsVisible;
            
            NoteSoundEditor.IsVisible = CosmeticSystem.CosmeticItem is NoteSound;
            NoteSoundEditor.IsEnabled = NoteSoundEditor.IsVisible;
            
            PlateEditor.IsVisible = CosmeticSystem.CosmeticItem is Plate;
            PlateEditor.IsEnabled = PlateEditor.IsVisible;
            
            SystemMusicEditor.IsVisible = CosmeticSystem.CosmeticItem is SystemMusic;
            SystemMusicEditor.IsEnabled = SystemMusicEditor.IsVisible;
            
            SystemSoundEditor.IsVisible = CosmeticSystem.CosmeticItem is SystemSound;
            SystemSoundEditor.IsEnabled = SystemSoundEditor.IsVisible;
            
            TitleEditor.IsVisible = CosmeticSystem.CosmeticItem is Title;
            TitleEditor.IsEnabled = TitleEditor.IsVisible;
        });
        
        UpdateNavigator();
        UpdateProfile();
        UpdateConsoleColor();
    }
#endregion System Event Handlers
    
#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        keyDownEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        keyUpEventHandler = KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        NavigatorExpressionRefresh += OnNavigatorExpressionRefresh;
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        NavigatorExpressionRefresh -= OnNavigatorExpressionRefresh;
        keyDownEventHandler?.Dispose();
        keyUpEventHandler?.Dispose();
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;
        
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Save"]))
        {
            _ = File_Save();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.SaveAs"]))
        {
            _ = File_SaveAs();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.ReloadFromDisk"]))
        {
            _ = File_ReloadFromDisk();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["File.Quit"]))
        {
            File_Quit();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"]))
        {
            UndoRedoSystem.CosmeticBranch.Undo();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"]))
        {
            UndoRedoSystem.CosmeticBranch.Redo();
            e.Handled = true;
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!IsEnabled) return;
        
        e.Handled = true;
    }
    
    
    private void MenuItemNewConsoleColor_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.ConsoleColor);
    private void MenuItemNewEmblem_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.Emblem);
    private void MenuItemNewIcon_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.Icon);
    private void MenuItemNewNavigator_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.Navigator);
    private void MenuItemNewNoteSound_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.NoteSound);
    private void MenuItemNewPlate_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.Plate);
    private void MenuItemNewSystemMusic_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.SystemMusic);
    private void MenuItemNewSystemSound_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.SystemSound);
    private void MenuItemNewTitle_OnClick(object? sender, RoutedEventArgs e) => File_New(CosmeticType.Title);
    
    private void MenuItemOpenConsoleColor_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.ConsoleColor);
    private void MenuItemOpenEmblem_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.Emblem);
    private void MenuItemOpenIcon_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.Icon);
    private void MenuItemOpenNavigator_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.Navigator);
    private void MenuItemOpenNoteSound_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.NoteSound);
    private void MenuItemOpenPlate_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.Plate);
    private void MenuItemOpenSystemMusic_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.SystemMusic);
    private void MenuItemOpenSystemSound_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.SystemSound);
    private void MenuItemOpenTitle_OnClick(object? sender, RoutedEventArgs e) => _ = File_Open(CosmeticType.Title);

    private void MenuItemSave_OnClick(object? sender, RoutedEventArgs e) => _ = File_Save();

    private void MenuItemSaveAs_OnClick(object? sender, RoutedEventArgs e) => _ = File_SaveAs();

    private void MenuItemReloadFromDisk_OnClick(object? sender, RoutedEventArgs e) => _ = File_ReloadFromDisk();

    private void MenuItemSettings_OnClick(object? sender, RoutedEventArgs e) => MainWindow.Instance?.ShowSettingsWindow();

    private void MenuItemQuit_OnClick(object? sender, RoutedEventArgs e) => File_Quit();


    private void MenuItemUndo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.CosmeticBranch.Undo();

    private void MenuItemRedo_OnClick(object? sender, RoutedEventArgs e) => UndoRedoSystem.CosmeticBranch.Redo();
    
    
    private void RadioButtonExpression_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton button) return;
        if (button.IsChecked == false) return;

        if (button == RadioButtonExpressionNeutral)
        {
            navigatorExpression = NavigatorExpression.Neutral;
        }
        else if (button == RadioButtonExpressionAmazed)
        {
            navigatorExpression = NavigatorExpression.Amazed;
        }
        else if (button == RadioButtonExpressionTroubled)
        {
            navigatorExpression = NavigatorExpression.Troubled;
        }
        else if (button == RadioButtonExpressionSurprised)
        {
            navigatorExpression = NavigatorExpression.Surprised;
        }
        else if (button == RadioButtonExpressionStartled)
        {
            navigatorExpression = NavigatorExpression.Startled;
        }
        else if (button == RadioButtonExpressionAngry)
        {
            navigatorExpression = NavigatorExpression.Angry;
        }
        else if (button == RadioButtonExpressionLaughing)
        {
            navigatorExpression = NavigatorExpression.Laughing;
        }
        else if (button == RadioButtonExpressionSmiling)
        {
            navigatorExpression = NavigatorExpression.Smiling;
        }
        else if (button == RadioButtonExpressionGrinning)
        {
            navigatorExpression = NavigatorExpression.Grinning;
        }
    }
#endregion UI Event Handlers

#region Internval Event Handlers
    private static void NavigatorTimer_OnTick(object? sender)
    {
        if (CosmeticSystem.CosmeticItem is not Navigator navigator)
        {
            navigatorTime = 0;
            return;
        }
        
        navigatorTime += 10;
        
        if (navigatorTime > navigator.BlinkAnimationInterval + navigator.BlinkAnimationDuration)
        {
            navigatorTime = 0;
        }
        
        if (navigatorTime > navigator.BlinkAnimationInterval + navigator.BlinkAnimationDuration * 0.66f)
        {
            navigatorBlinkState = NavigatorBlinkState.HalfClose;
        }
        else if (navigatorTime > navigator.BlinkAnimationInterval + navigator.BlinkAnimationDuration * 0.33f)
        {
            navigatorBlinkState = NavigatorBlinkState.Closed;
        }
        else if (navigatorTime > navigator.BlinkAnimationInterval)
        {
            navigatorBlinkState = NavigatorBlinkState.HalfClose;
        }
        else
        {
            navigatorBlinkState = NavigatorBlinkState.Open;
        }

        NavigatorExpressionRefresh?.Invoke(null, EventArgs.Empty);
    }

    private void OnNavigatorExpressionRefresh(object? sender, EventArgs e) => UpdateNavigatorExpression();
#endregion Internal Event Handlers
}