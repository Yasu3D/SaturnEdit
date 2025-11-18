using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        ActualThemeVariantChanged += Control_OnActualThemeVariantChanged;
        
        TextEditorChart.Options.ConvertTabsToSpaces = true;
        TextEditorChart.Options.EnableTextDragDrop = true;
        
        Task.Delay(5); // Hacky
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartSystem.EntryChanged += OnEntryChanged;
        UpdateTextFromChart();
    }
    
    private TextMate.Installation? installation;
    private readonly NotationWriteArgs writeArgs;
    private readonly NotationReadArgs readArgs;
    
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
        ToggleButtonShowSpaces.IsChecked = SettingsSystem.EditorSettings.ChartViewTxtShowSpaces;
        ToggleButtonSyntaxHighlighting.IsChecked = SettingsSystem.EditorSettings.ChartViewTxtSyntaxHighlighting;
    }
    
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e) => UpdateTextFromChart();

    private void OnEntryChanged(object? sender, EventArgs e) => UpdateTextFromChart();
#endregion System Event Handlers

#region UI Event Handlers
    private void Control_OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ThemeName themeName = SettingsSystem.EditorSettings.Theme switch
        {
            EditorSettings.EditorThemeOptions.Light => ThemeName.LightPlus,
            EditorSettings.EditorThemeOptions.Dark => ThemeName.DarkPlus,
            _ => ThemeName.DarkPlus,
        };
        
        RegistryOptions registryOptions = new(themeName);
        installation = TextEditorChart.InstallTextMate(registryOptions);
        installation.SetGrammarFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/sat.tmLanguage.json"));
    }
    
    private void ButtonApplyChanges_OnClick(object? sender, RoutedEventArgs e) => UpdateChartFromText();
    
    private void ToggleButtonShowSpaces_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (TextEditorChart == null || ToggleButtonShowSpaces == null) return;

        TextEditorChart.Options.ShowSpaces = ToggleButtonShowSpaces.IsChecked ?? false;
        SettingsSystem.EditorSettings.ChartViewTxtShowSpaces = ToggleButtonShowSpaces.IsChecked ?? false;
    }

    private void ToggleButtonSyntaxHighlighting_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (installation == null || ToggleButtonSyntaxHighlighting == null) return;

        try
        {
            if (ToggleButtonSyntaxHighlighting.IsChecked ?? true)
            {
                installation.SetGrammar("source.sat");
            }
            else
            {
                installation.SetGrammar(null);
            }

            SettingsSystem.EditorSettings.ChartViewTxtSyntaxHighlighting = ToggleButtonSyntaxHighlighting.IsChecked ?? false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
#endregion UI Event Handlers
}