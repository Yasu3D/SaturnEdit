using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TextMateSharp.Grammars;
using AvaloniaEdit.TextMate;
using System.IO;
using Avalonia.Threading;
using SaturnData.Notation.Serialization;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartViewTxt : UserControl
{
    public ChartViewTxt()
    {
        InitializeComponent();

        writeArgs = new();
        readArgs = new();
        
        TextEditorChart.Options.ConvertTabsToSpaces = true;
        TextEditorChart.Options.EnableTextDragDrop = true;
    }
    
    private TextMate.Installation? installation;
    private readonly NotationWriteArgs writeArgs;
    private readonly NotationReadArgs readArgs;

    private bool blockEvents = false;
    
#region Methods
    private void UpdateChartFromText()
    {
        ChartSystem.ReadChartEditorTxt(TextEditorChart.Text, ChartSystem.Entry.RootDirectory, readArgs, out List<Exception> exceptions);
        
        Dispatcher.UIThread.Post(() =>
        {
            ErrorList.IsVisible = exceptions.Count > 0;
            TextBlockErrorCount.Text = $"Errors found in file : {exceptions.Count}";
            TextBlockErrorList.Text = "";
            foreach (Exception exception in exceptions)
            {
                TextBlockErrorList.Text += exception.Message + "\n";
            }
        });
    }
    
    private void UpdateTextFromChart()
    {
        string text = NotationSerializer.ToString(ChartSystem.Entry, ChartSystem.Chart, writeArgs);
        
        Dispatcher.UIThread.Post(() =>
        {
            TextEditorChart.Text = text;
        });
    }
#endregion Methods

#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            ToggleButtonShowSpaces.IsChecked = SettingsSystem.EditorSettings.ChartViewTxtShowSpaces;
            ToggleButtonSyntaxHighlighting.IsChecked = SettingsSystem.EditorSettings.ChartViewTxtSyntaxHighlighting;

            blockEvents = false;
            
            TextEditorChart.Options.ShowSpaces = ToggleButtonShowSpaces.IsChecked ?? false;
            
            try
            {
                if (ToggleButtonSyntaxHighlighting.IsChecked ?? true)
                {
                    installation?.SetGrammar("source.sat");
                }
                else
                {
                    installation?.SetGrammar(null);
                }
            }
            catch (Exception ex)
            {
                // Don't throw.
                Console.WriteLine(ex);
            }
        });
    }
    
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e) => UpdateTextFromChart();

    private void OnEntryChanged(object? sender, EventArgs e) => UpdateTextFromChart();
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        Control_OnActualThemeVariantChanged(null, EventArgs.Empty);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartSystem.EntryChanged += OnEntryChanged;
        UpdateTextFromChart();
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        ActualThemeVariantChanged -= Control_OnActualThemeVariantChanged;
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        UndoRedoSystem.ChartBranch.OperationHistoryChanged -= ChartBranch_OnOperationHistoryChanged;
        ChartSystem.EntryChanged -= OnEntryChanged;
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ThemeName themeName = SettingsSystem.EditorSettings.Theme switch
        {
            EditorSettings.EditorThemeOption.Light => ThemeName.LightPlus,
            EditorSettings.EditorThemeOption.Dark => ThemeName.DarkPlus,
            _ => ThemeName.DarkPlus,
        };
        
        RegistryOptions registryOptions = new(themeName);
        installation = TextEditorChart.InstallTextMate(registryOptions);
        installation.SetGrammarFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/sat.tmLanguage.json"));
    }
    
    private void ButtonApplyChanges_OnClick(object? sender, RoutedEventArgs e) => UpdateChartFromText();
    
    private void ToggleButtonShowSpaces_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextEditorChart == null || ToggleButtonShowSpaces == null) return;
        
        SettingsSystem.EditorSettings.ChartViewTxtShowSpaces = ToggleButtonShowSpaces.IsChecked ?? false;
    }

    private void ToggleButtonSyntaxHighlighting_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (installation == null || ToggleButtonSyntaxHighlighting == null) return;

        SettingsSystem.EditorSettings.ChartViewTxtSyntaxHighlighting = ToggleButtonSyntaxHighlighting.IsChecked ?? false;
    }
#endregion UI Event Handlers
}