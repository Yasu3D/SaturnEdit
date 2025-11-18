using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using ConsoleColor = SaturnData.Content.Cosmetics.ConsoleColor;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class ConsoleColorEditorView : UserControl
{
    public ConsoleColorEditorView()
    {
        InitializeComponent();
        
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Delegates
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxColorA.Text = $"{consoleColor.ColorA - 0xFF000000:X6}";
            TextBoxColorB.Text = $"{consoleColor.ColorB - 0xFF000000:X6}";
            TextBoxColorC.Text = $"{consoleColor.ColorC - 0xFF000000:X6}";
            TextBoxLedA.Text = $"{consoleColor.LedA - 0xFF000000:X6}";
            TextBoxLedB.Text = $"{consoleColor.LedB - 0xFF000000:X6}";
            TextBoxLedC.Text = $"{consoleColor.LedC - 0xFF000000:X6}";

            ColorPickerColorA.Color = consoleColor.ColorA;
            ColorPickerColorB.Color = consoleColor.ColorB;
            ColorPickerColorC.Color = consoleColor.ColorC;
            ColorPickerLedA.Color = consoleColor.LedA;
            ColorPickerLedB.Color = consoleColor.LedB;
            ColorPickerLedC.Color = consoleColor.LedC;
            
            blockEvents = false;
        });
    }
#endregion System Event Delegates
    
#region UI Event Delegates
    private void TextBoxColorA_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        uint newValue = uint.TryParse(TextBoxColorA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
    }

    private void TextBoxLedA_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        uint newValue = uint.TryParse(TextBoxLedA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
    }
    
    private void TextBoxColorB_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        uint newValue = uint.TryParse(TextBoxColorB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
    }
    
    private void TextBoxLedB_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        uint newValue = uint.TryParse(TextBoxLedB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
    }
    
    private void TextBoxColorC_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        uint newValue = uint.TryParse(TextBoxColorC.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
    }
    
    private void TextBoxLedC_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        
        uint newValue = uint.TryParse(TextBoxLedC.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;
    }

    
    private void ColorPickerColorA_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
    }
    
    private void ColorPickerLedA_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
    }
    
    private void ColorPickerColorB_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
    }
    
    private void ColorPickerLedB_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
    }

    private void ColorPickerColorC_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
    }

    private void ColorPickerLedC_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
    }


    private void ColorPickerColorA_OnColorChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        TextBoxColorA.Text = $"{ColorPickerColorA.Color - 0xFF000000:X6}";
        
        blockEvents = false;
    }

    private void ColorPickerLedA_OnColorChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        TextBoxColorB.Text = $"{ColorPickerColorB.Color - 0xFF000000:X6}";
        
        blockEvents = false;
    }

    private void ColorPickerColorB_OnColorChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        TextBoxColorC.Text = $"{ColorPickerColorC.Color - 0xFF000000:X6}";
        
        blockEvents = false;
    }

    private void ColorPickerLedB_OnColorChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        TextBoxLedA.Text = $"{ColorPickerLedA.Color - 0xFF000000:X6}";

        blockEvents = false;
    }

    private void ColorPickerColorC_OnColorChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        TextBoxLedB.Text = $"{ColorPickerLedB.Color - 0xFF000000:X6}";

        blockEvents = false;
    }

    private void ColorPickerLedC_OnColorChanged(object? sender, EventArgs e)
    {
        blockEvents = true;

        TextBoxLedC.Text = $"{ColorPickerLedC.Color - 0xFF000000:X6}";

        blockEvents = false;
    }
#endregion UI Event Delegates
}