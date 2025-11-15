using System;
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

        KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        UndoRedoSystem.StageBranch.OperationHistoryChanged += StageBranch_OnOperationHistoryChanged;
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        UpdateUndoRedoButtons();
        
        Closed += AudioSystem.OnClosed;
        Closing += ChartEditor.OnClosing;
        Closing += Window_OnClosing;

        if (Application.Current != null)
        {
            Application.Current.TryGetResource("ChartEditorChromeGradient", Application.Current.ActualThemeVariant, out object? resource);
            chartEditorChromeGradient = (IBrush?)resource;
            
            Application.Current.TryGetResource("StageEditorChromeGradient", Application.Current.ActualThemeVariant, out resource);
            stageEditorChromeGradient = (IBrush?)resource;
            
            Application.Current.TryGetResource("CosmeticsEditorChromeGradient", Application.Current.ActualThemeVariant, out resource);
            cosmeticsEditorChromeGradient = (IBrush?)resource;
        }
        
        UpdateTabs();
    }

    public static MainWindow? Instance { get; private set; }

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
    
    private readonly IBrush? chartEditorChromeGradient = null;
    private readonly IBrush? stageEditorChromeGradient = null;
    private readonly IBrush? cosmeticsEditorChromeGradient = null;

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
            
            if (chartEditor)
            {
                RectangleChromeGradient.Fill = chartEditorChromeGradient;
            }
            else if (stageEditor)
            {
                RectangleChromeGradient.Fill = stageEditorChromeGradient;
            }
            else if (cosmeticsEditor)
            {
                RectangleChromeGradient.Fill = cosmeticsEditorChromeGradient;
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
            WindowTitleKey = "ModalDialog.SavePrompt.Title",
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
#endregion Methods

#region System Event Delegates
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
    }
#endregion System Event Delegates

#region UI Event Delegates
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

    private void ButtonUpdate_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ButtonUndo_OnClick(object? sender, RoutedEventArgs e) => Undo();
    
    private void ButtonRedo_OnClick(object? sender, RoutedEventArgs e) => Redo();
    
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

            // TODO: Cosmetics Editor
            if (!bypassCosmeticsSave)
            {
                
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
#endregion UI Event Delegates
}