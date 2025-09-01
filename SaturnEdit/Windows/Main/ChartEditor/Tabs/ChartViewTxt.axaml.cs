using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using TextMateSharp.Grammars;
using AvaloniaEdit.TextMate;
using System.IO;
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

        RegistryOptions registryOptions = new(ThemeName.DarkPlus); // TODO: Light/Dark Mode support!
        installation = TextEditorChart.InstallTextMate(registryOptions);
        installation.SetGrammarFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/sat.tmLanguage.json"));
        
        TextEditorChart.Options.ConvertTabsToSpaces = true;
        TextEditorChart.Options.EnableTextDragDrop = true;
        
        Task.Delay(5); // Hacky
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        ChartSystem.ChartChanged += OnChartChanged;
        ChartSystem.EntryChanged += OnEntryChanged;
        UpdateTextFromChart();
    }
    
    private readonly TextMate.Installation? installation;

    private readonly NotationWriteArgs writeArgs;
    private readonly NotationReadArgs readArgs;
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        ToggleButtonShowSpaces.IsChecked = SettingsSystem.EditorSettings.ChartViewTxtShowSpaces;
        ToggleButtonSyntaxHighlighting.IsChecked = SettingsSystem.EditorSettings.ChartViewTxtSyntaxHighlighting;
    }
    
    private void OnChartChanged(object? sender, EventArgs e) => UpdateTextFromChart();

    private void OnEntryChanged(object? sender, EventArgs e) => UpdateTextFromChart();
    
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

    private void UpdateChartFromText()
    {
        if (!ChartSystem.IsSaved)
        {
            
        }

        string[] data = TextEditorChart.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        ChartSystem.ReadChart(data, readArgs);
    }

    private void UpdateTextFromChart()
    {
        TextEditorChart.Text = NotationSerializer.ToString(ChartSystem.Entry, ChartSystem.Chart, writeArgs);
    }
}