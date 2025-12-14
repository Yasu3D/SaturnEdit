using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SaturnEdit.Windows.Dialogs.Search;
using SaturnEdit.Windows.Dialogs.VolumeMixer;

namespace SaturnEdit;

public enum SavePromptType
{
    Chart = 0,
    Stage = 1,
    Cosmetic = 2,
}

public partial class MainWindow : Window
{
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();
    }

    public static MainWindow? Instance { get; private set; }
    
    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;

    private bool CanUndo
    {
        get
        {
            if (ChartEditor.IsVisible) return UndoRedoSystem.ChartBranch.CanUndo;
            if (StageEditor.IsVisible) return UndoRedoSystem.StageBranch.CanUndo;
            if (CosmeticsEditor.IsVisible) return UndoRedoSystem.CosmeticBranch.CanUndo;

            return false;
        }
    }
    
    private bool CanRedo
    {
        get
        {
            if (ChartEditor.IsVisible) return UndoRedoSystem.ChartBranch.CanRedo;
            if (StageEditor.IsVisible) return UndoRedoSystem.StageBranch.CanRedo;
            if (CosmeticsEditor.IsVisible) return UndoRedoSystem.CosmeticBranch.CanRedo;

            return false;
        }
    }

    private bool bypassChartSave = false;
    private bool bypassStageSave = false;
    private bool bypassCosmeticsSave = false;
    
#region Methods
    public async void ShowSettingsWindow()
    {
        try
        {
            SettingsWindow settingsWindow = new();
            settingsWindow.Position = DialogPopupPosition(settingsWindow.Width, settingsWindow.Height);

            await settingsWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async void ShowSearchWindow()
    {
        try
        {
            SearchWindow searchWindow = new();
            searchWindow.Position = DialogPopupPosition(searchWindow.Width, searchWindow.Height);

            await searchWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async void ShowVolumeMixerWindow()
    {
        try
        {
            VolumeMixerWindow volumeMixerWindow = new();
            volumeMixerWindow.Position = DialogPopupPosition(volumeMixerWindow.Width, volumeMixerWindow.Height);

            await volumeMixerWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    
    private void UpdateTabs()
    {
        Dispatcher.UIThread.Post(() =>
        {
            bool chartEditor = TabChartEditor.IsChecked ?? false;
            bool stageEditor = TabStageEditor.IsChecked ?? false;
            bool cosmeticsEditor = TabCosmeticsEditor.IsChecked ?? false;
            
            ChartEditor.IsEnabled = chartEditor;
            ChartEditor.IsVisible = chartEditor;
            
            StageEditor.IsEnabled = stageEditor;
            StageEditor.IsVisible = stageEditor;
                
            CosmeticsEditor.IsEnabled = cosmeticsEditor;
            CosmeticsEditor.IsVisible = cosmeticsEditor;
            
            if (Application.Current == null) return;
            
            if (chartEditor)
            {
                Application.Current.TryGetResource("ChartEditorChromeGradient", Application.Current.ActualThemeVariant, out object? resource);
                RectangleChromeGradient.Fill = (IBrush?)resource;
            }
            else if (stageEditor)
            {
                Application.Current.TryGetResource("StageEditorChromeGradient", Application.Current.ActualThemeVariant, out object? resource);
                RectangleChromeGradient.Fill = (IBrush?)resource;
            }
            else if (cosmeticsEditor)
            {
                Application.Current.TryGetResource("CosmeticsEditorChromeGradient", Application.Current.ActualThemeVariant, out object? resource);
                RectangleChromeGradient.Fill = (IBrush?)resource;
            }
        });
    }

    private void UpdateUndoRedoButtons()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ButtonUndo.IsEnabled = CanUndo;
            ButtonRedo.IsEnabled = CanRedo;
        });
    }

    private void UpdateSplash()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        TextBlockVersion.Text = versionInfo.FileVersion;
        
        string file;
        string trimmed;

        if (SettingsSystem.EditorSettings.RecentChartFiles.Count >= 1)
        {
            file = SettingsSystem.EditorSettings.RecentChartFiles[^1];
            trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";
            TextBlockRecentFile1.Text = trimmed;
            
            ButtonRecentFile1.IsVisible = true;
        }
        else
        {
            ButtonRecentFile1.IsVisible = false;
        }
        
        if (SettingsSystem.EditorSettings.RecentChartFiles.Count >= 2)
        {
            file = SettingsSystem.EditorSettings.RecentChartFiles[^2];
            trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";
            TextBlockRecentFile2.Text = trimmed;
            
            ButtonRecentFile2.IsVisible = true;
        }
        else
        {
            ButtonRecentFile2.IsVisible = false;
        }
        
        if (SettingsSystem.EditorSettings.RecentChartFiles.Count >= 3)
        {
            file = SettingsSystem.EditorSettings.RecentChartFiles[^3];
            trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";
            TextBlockRecentFile3.Text = trimmed;
            
            ButtonRecentFile3.IsVisible = true;
        }
        else
        {
            ButtonRecentFile3.IsVisible = false;
        }
        
        if (SettingsSystem.EditorSettings.RecentChartFiles.Count >= 4)
        {
            file = SettingsSystem.EditorSettings.RecentChartFiles[^4];
            trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";
            TextBlockRecentFile4.Text = trimmed;
            
            ButtonRecentFile4.IsVisible = true;
        }
        else
        {
            ButtonRecentFile4.IsVisible = false;
        }
        
        if (SettingsSystem.EditorSettings.RecentChartFiles.Count >= 5)
        {
            file = SettingsSystem.EditorSettings.RecentChartFiles[^5];
            trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";
            TextBlockRecentFile5.Text = trimmed;
            
            ButtonRecentFile5.IsVisible = true;
        }
        else
        {
            ButtonRecentFile5.IsVisible = false;
        }
        
        if (SettingsSystem.EditorSettings.RecentChartFiles.Count >= 6)
        {
            file = SettingsSystem.EditorSettings.RecentChartFiles[^6];
            trimmed = $"{Path.GetFileName(Path.GetDirectoryName(file))}/{Path.GetFileName(file)}";
            TextBlockRecentFile6.Text = trimmed;
            
            ButtonRecentFile6.IsVisible = true;
        }
        else
        {
            ButtonRecentFile6.IsVisible = false;
        }
        
        Splash.IsVisible = SettingsSystem.EditorSettings.ShowSplashScreen;
    }

    private async void UpdateUpdateButton()
    {
        bool updateAvailable = await SoftwareUpdateSystem.UpdateAvailable();
        
        Dispatcher.UIThread.Post(() =>
        {
            ButtonUpdate.IsVisible = updateAvailable;
        });
    }

    private void Undo()
    {
        if (ChartEditor.IsVisible) UndoRedoSystem.ChartBranch.Undo();
        if (StageEditor.IsVisible) UndoRedoSystem.StageBranch.Undo();
        if (CosmeticsEditor.IsVisible) UndoRedoSystem.CosmeticBranch.Undo();
    }

    private void Redo()
    {
        if (ChartEditor.IsVisible) UndoRedoSystem.ChartBranch.Redo();
        if (StageEditor.IsVisible) UndoRedoSystem.StageBranch.Redo();
        if (CosmeticsEditor.IsVisible) UndoRedoSystem.CosmeticBranch.Redo();
    }
    
    public static PixelPoint DialogPopupPosition(double width, double height)
    {
        return Instance == null
            ? new(0, 0)
            : new((int)(Instance.Position.X + Instance.Bounds.Width / 2 - width / 2),
                (int)(Instance.Position.Y + Instance.Bounds.Height / 2 - height / 2));
    }
    
    public static async Task<ModalDialogResult> ShowSavePrompt(SavePromptType type)
    {
        if (Instance == null) return ModalDialogResult.Cancel;
        
        ModalDialogWindow dialog = new()
        {
            DialogIcon = FluentIcons.Common.Icon.Warning,
            WindowTitleKey = type switch
            {
                SavePromptType.Chart => "ModalDialog.SavePrompt.Title.Chart",
                SavePromptType.Stage => "ModalDialog.SavePrompt.Title.Stage",
                SavePromptType.Cosmetic => "ModalDialog.SavePrompt.Title.Cosmetic",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            },
            HeaderKey = type switch
            {
                SavePromptType.Chart => "ModalDialog.SavePrompt.Header.Chart",
                SavePromptType.Stage => "ModalDialog.SavePrompt.Header.Stage",
                SavePromptType.Cosmetic => "ModalDialog.SavePrompt.Header.Cosmetic",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            },
            ParagraphKey = "ModalDialog.SavePrompt.Paragraph",
            ButtonPrimaryKey = "Menu.File.Save",
            ButtonSecondaryKey = "ModalDialog.SavePrompt.DontSave",
            ButtonTertiaryKey = "Generic.Cancel",
        };
        
        dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);

        dialog.InitializeDialog();
        await dialog.ShowDialog(Instance);
        return dialog.Result;
    }
    
    public static void ShowFileWriteError()
    {
        if (Instance == null) return;
        
        ModalDialogWindow dialog = new()
        {
            DialogIcon = FluentIcons.Common.Icon.Warning,
            WindowTitleKey = "ModalDialog.FileWriteError.Title",
            HeaderKey = "ModalDialog.FileWriteError.Header",
            ParagraphKey = "ModalDialog.FileWriteError.Paragraph",
            ButtonPrimaryKey = "Generic.Ok",
        };
        
        dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);

        dialog.InitializeDialog();
        dialog.ShowDialog(Instance);
    }
    
    public static async Task<bool> PromptFileMoveAndOverwrite(string sourceFilePath, string projectFilePath)
    {
        if (sourceFilePath != projectFilePath)
        {
            ModalDialogResult moveResult = await promptFileMove();
            if (moveResult is ModalDialogResult.Tertiary or ModalDialogResult.Cancel) return false;

            // File already exists in root directory. Prompt to overwrite files.
            if (File.Exists(projectFilePath))
            {
                ModalDialogResult overwriteResult = await promptFileOverwrite();
                if (overwriteResult is ModalDialogResult.Secondary or ModalDialogResult.Cancel) return false;
            }
            
            if (moveResult is ModalDialogResult.Primary)
            {
                // Copy
                File.Copy(sourceFilePath, projectFilePath, true);
            }
            else if (moveResult is ModalDialogResult.Secondary)
            {
                // Move
                File.Move(sourceFilePath, projectFilePath, true);
            }
        }

        return true;
        
        async Task<ModalDialogResult> promptFileMove()
        {
            if (Instance == null) return ModalDialogResult.Cancel;

            ModalDialogWindow dialog = new()
            {
                DialogIcon = FluentIcons.Common.Icon.Warning,
                WindowTitleKey = "ModalDialog.FileMovePrompt.Title",
                HeaderKey = "ModalDialog.FileMovePrompt.Header",
                ParagraphKey = "ModalDialog.FileMovePrompt.Paragraph",
                ButtonPrimaryKey = "ModalDialog.FileMovePrompt.CopyFile",
                ButtonSecondaryKey = "ModalDialog.FileMovePrompt.MoveFile",
                ButtonTertiaryKey = "Generic.Cancel",
            };
            
            dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);

            dialog.InitializeDialog();
            await dialog.ShowDialog(Instance);
            return dialog.Result;
        }
        
        async Task<ModalDialogResult> promptFileOverwrite()
        {
            if (Instance == null) return ModalDialogResult.Cancel;

            ModalDialogWindow dialog = new()
            {
                DialogIcon = FluentIcons.Common.Icon.Warning,
                WindowTitleKey = "ModalDialog.FileOverwritePrompt.Title",
                HeaderKey = "ModalDialog.FileOverwritePrompt.Header",
                ParagraphKey = "ModalDialog.FileOverwritePrompt.Paragraph",
                ButtonPrimaryKey = "ModalDialog.FileOverwritePrompt.OverwriteFile",
                ButtonSecondaryKey = "Generic.Cancel",
            };
            
            dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);

            dialog.InitializeDialog();
            await dialog.ShowDialog(Instance);
            return dialog.Result;
        }
    }
#endregion Methods

#region System Event Handlers
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e) => UpdateUndoRedoButtons();
    private void StageBranch_OnOperationHistoryChanged(object? sender, EventArgs e) => UpdateUndoRedoButtons();
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e) => UpdateUndoRedoButtons();

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockShortcutSearch.Text = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Search"].ToString();
            TextBlockShortcutVolumeMixer.Text = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.VolumeMixer"].ToString();
            TextBlockShortcutSettings.Text = SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"].ToString();
            TextBlockShortcutUndo.Text = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Undo"].ToString();
            TextBlockShortcutRedo.Text = SettingsSystem.ShortcutSettings.Shortcuts["Edit.Redo"].ToString();
        });
        
        UpdateTabs();
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        keyDownEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        keyUpEventHandler = KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        UndoRedoSystem.StageBranch.OperationHistoryChanged += StageBranch_OnOperationHistoryChanged;
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        UpdateUndoRedoButtons();
        
        Closed += AudioSystem.OnClosed;
        Closed += AutosaveSystem.OnClosed;
        Closing += ChartEditor.OnClosing;
        Closing += Window_OnClosing;
        
        UpdateTabs();
        UpdateSplash();
        UpdateUpdateButton();
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        UndoRedoSystem.ChartBranch.OperationHistoryChanged -= ChartBranch_OnOperationHistoryChanged;
        UndoRedoSystem.StageBranch.OperationHistoryChanged -= StageBranch_OnOperationHistoryChanged;
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        Closed -= AudioSystem.OnClosed;
        Closed -= AutosaveSystem.OnClosed;
        Closing -= ChartEditor.OnClosing;
        Closing -= Window_OnClosing;
        keyDownEventHandler?.Dispose();
        keyUpEventHandler?.Dispose();
        
        base.OnUnloaded(e);
    }
    
    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        UpdateTabs();
    }
    
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Settings"]))
        {
            ShowSettingsWindow();
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.VolumeMixer"]))
        {
            ShowVolumeMixerWindow();
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["QuickCommands.Search"]))
        {
            ShowSearchWindow();
        }
        
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private void EditorTabs_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        UpdateTabs();
        UpdateUndoRedoButtons();
    }

    private void ButtonSearch_OnClick(object? sender, RoutedEventArgs e) => ShowSearchWindow();

    private void ButtonVolumeMixer_OnClick(object? sender, RoutedEventArgs e) => ShowVolumeMixerWindow();
    
    private void ButtonSettings_OnClick(object? sender, RoutedEventArgs e) => ShowSettingsWindow();

    private async void ButtonUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ModalDialogWindow dialog = new()
            {
                DialogIcon = FluentIcons.Common.Icon.ArrowDownload,
                WindowTitleKey = "ModalDialog.Update.Title",
                HeaderKey = "ModalDialog.Update.Header",
                ParagraphKey = "ModalDialog.Update.Paragraph",
                ButtonPrimaryKey = "Generic.Yes",
                ButtonSecondaryKey = "Generic.No",
            };

            dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);
            dialog.InitializeDialog();
            await dialog.ShowDialog(this);

            if (dialog.Result != ModalDialogResult.Primary) return;

            ModalDialogResult result;
            if (!ChartSystem.IsSaved)
            {
                result = await ShowSavePrompt(SavePromptType.Chart);
                if (result is ModalDialogResult.Primary)
                {
                    bool saved = await ChartEditor.File_Save();

                    if (!saved) return;
                }
                else if (result is not ModalDialogResult.Secondary)
                {
                    return;
                }
            }

            if (!StageSystem.IsSaved)
            {
                result = await ShowSavePrompt(SavePromptType.Stage);
                if (result is ModalDialogResult.Primary)
                {
                    bool saved = await StageEditor.File_Save();

                    if (!saved) return;
                }
                else if (result is not ModalDialogResult.Secondary)
                {
                    return;
                }
            }

            if (!CosmeticSystem.IsSaved)
            {
                result = await ShowSavePrompt(SavePromptType.Cosmetic);
                if (result is ModalDialogResult.Primary)
                {
                    bool saved = await CosmeticsEditor.File_Save();

                    if (!saved) return;
                }
                else if (result is not ModalDialogResult.Secondary)
                {
                    return;
                }
            }

            // Show "update in progress" dialog.
            dialog = new()
            {
                DialogIcon = FluentIcons.Common.Icon.HourglassThreeQuarter,
                WindowTitleKey = "ModalDialog.Update.Title",
                HeaderKey = "ModalDialog.Update.Active.Header",
                ParagraphKey = "ModalDialog.Update.Active.Paragraph",
                CanClose = false,
            };
            
            dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);
            dialog.InitializeDialog();
            _ = dialog.ShowDialog(this);
            
            (bool, string) updateStatus = await SoftwareUpdateSystem.Update();

            // If this point is reached, something exploded.
            // For UX reasons, add a ~1 second "fake" wait before showing the error dialog, to not have a window flicker on and off instantly.
            await Task.Delay(1000);
            
            // Show error dialog.
            if (!updateStatus.Item1)
            {
                dialog.Close();
                
                dialog = new()
                {
                    DialogIcon = FluentIcons.Common.Icon.Warning,
                    WindowTitleKey = "ModalDialog.Update.Title",
                    HeaderKey = "ModalDialog.Update.Error.Header",
                    ParagraphKey = updateStatus.Item2,
                    ButtonPrimaryKey = "Generic.Ok",
                };
                
                dialog.Position = DialogPopupPosition(dialog.Width, dialog.Height);
                dialog.InitializeDialog();
                _ = dialog.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e) => Undo();
    
    private void ButtonRedo_OnClick(object? sender, RoutedEventArgs e) => Redo();
    
    private void ButtonSplashNewChart_OnClick(object? sender, RoutedEventArgs e)
    {
        Splash.IsVisible = false;
        ChartEditor.File_New();
    }

    private void ButtonSplashOpenChart_OnClick(object? sender, RoutedEventArgs e)
    {
        Splash.IsVisible = false;
        _ = ChartEditor.File_Open();
    }

    private void ButtonSplashRecoverLastSession_OnClick(object? sender, RoutedEventArgs e)
    {
        Splash.IsVisible = false;

        _ = ChartEditor.File_OpenLastSession();
    }

    private void ButtonSplashRecentFile_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        Splash.IsVisible = false;

        int index = button.Name switch
        {
            "ButtonRecentFile1" => 1,
            "ButtonRecentFile2" => 2,
            "ButtonRecentFile3" => 3,
            "ButtonRecentFile4" => 4,
            "ButtonRecentFile5" => 5,
            "ButtonRecentFile6" => 6,
            _ => -1,
        };

        index = SettingsSystem.EditorSettings.RecentChartFiles.Count - index;

        if (index == -1) return;
        if (index >= SettingsSystem.EditorSettings.RecentChartFiles.Count) return;
        
        ChartSystem.ReadChart(SettingsSystem.EditorSettings.RecentChartFiles[index], new());
    }

    private void ButtonSplashWebsite_OnClick(object? sender, RoutedEventArgs e)
    {
        ProcessStartInfo process = new()
        {
            FileName = "https://saturn.yasu3d.art",
            UseShellExecute = true,
        };

        Process.Start(process);
    }

    private void ButtonSplashDocumentation_OnClick(object? sender, RoutedEventArgs e)
    {
        ProcessStartInfo process = new()
        {
            FileName = "https://saturn.yasu3d.art/docs/#/",
            UseShellExecute = true,
        };

        Process.Start(process);
    }

    private void ButtonSplashWhatsNew_OnClick(object? sender, RoutedEventArgs e)
    {
        ProcessStartInfo process = new()
        {
            FileName = "https://saturn.yasu3d.art/changelog",
            UseShellExecute = true,
        };

        Process.Start(process);
    }

    private void ButtonSplashGithub_OnClick(object? sender, RoutedEventArgs e)
    {
        ProcessStartInfo process = new()
        {
            FileName = "https://github.com/Yasu3D/SaturnEdit",
            UseShellExecute = true,
        };

        Process.Start(process);
    }

    private void SplashBackground_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed == false) return;
        
        Splash.IsVisible = false;
    }
    
    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            // Prompt the user to save unsaved work in...

            // Chart Editor
            if (!bypassChartSave && !ChartSystem.IsSaved)
            {
                e.Cancel = true;
                ModalDialogResult result = await ShowSavePrompt(SavePromptType.Chart);

                if (result is ModalDialogResult.Primary)
                {
                    bool saved = await ChartEditor.File_Save();

                    if (saved)
                    {
                        Close();
                        return;
                    }
                }
                else if (result is ModalDialogResult.Secondary)
                {
                    bypassChartSave = true;
                    Close();
                    return;
                }
            }

            // Stage Editor
            if (!bypassStageSave && !StageSystem.IsSaved)
            {
                e.Cancel = true;
                ModalDialogResult result = await ShowSavePrompt(SavePromptType.Stage);

                if (result is ModalDialogResult.Primary)
                {
                    bool saved = await StageEditor.File_Save();

                    if (saved)
                    {
                        Close();
                        return;
                    }
                }
                else if (result is ModalDialogResult.Secondary)
                {
                    bypassStageSave = true;
                    Close();
                    return;
                }
            }
            
            // Cosmetic Editor
            if (!bypassCosmeticsSave && !CosmeticSystem.IsSaved)
            {
                e.Cancel = true;
                ModalDialogResult result = await ShowSavePrompt(SavePromptType.Cosmetic);

                if (result is ModalDialogResult.Primary)
                {
                    bool saved = await CosmeticsEditor.File_Save();

                    if (saved)
                    {
                        Close();
                        return;
                    }
                }
                else if (result is ModalDialogResult.Secondary)
                {
                    bypassStageSave = true;
                    Close();
                    return;
                }
            }
            
            bypassChartSave = false;
            bypassStageSave = false;
            bypassCosmeticsSave = false;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Handlers
}