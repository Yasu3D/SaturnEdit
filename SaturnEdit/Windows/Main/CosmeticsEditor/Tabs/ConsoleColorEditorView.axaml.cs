using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.GenericOperations;
using ConsoleColor = SaturnData.Content.Cosmetics.ConsoleColor;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class ConsoleColorEditorView : UserControl
{
    public ConsoleColorEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
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
    
    private void TextBoxColorA_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.ColorA;
        uint newValue = uint.TryParse(TextBoxColorA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.ColorA = value; }, oldValue, newValue));
    }

    private void TextBoxLedA_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.LedA;
        uint newValue = uint.TryParse(TextBoxLedA.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.LedA = value; }, oldValue, newValue));
    }
    
    private void TextBoxColorB_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.ColorB;
        uint newValue = uint.TryParse(TextBoxColorB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.ColorB = value; }, oldValue, newValue));
    }
    
    private void TextBoxLedB_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.LedB;
        uint newValue = uint.TryParse(TextBoxLedB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.LedB = value; }, oldValue, newValue));
    }
    
    private void TextBoxColorC_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.ColorC;
        uint newValue = uint.TryParse(TextBoxColorC.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.ColorC = value; }, oldValue, newValue));
    }
    
    private void TextBoxLedC_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.LedC;
        uint newValue = uint.TryParse(TextBoxLedC.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result) ? result + 0xFF000000 : 0xFFDDDDDD;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.LedC = value; }, oldValue, newValue));
    }

    
    private void ColorPickerColorA_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.ColorA;
        uint newValue = ColorPickerColorA.Color;
        
        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.ColorA = value; }, oldValue, newValue));
    }
    
    private void ColorPickerLedA_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.LedA;
        uint newValue = ColorPickerLedA.Color;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.LedA = value; }, oldValue, newValue));
    }
    
    private void ColorPickerColorB_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.ColorB;
        uint newValue = ColorPickerColorB.Color;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.ColorB = value; }, oldValue, newValue));
    }
    
    private void ColorPickerLedB_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.LedB;
        uint newValue = ColorPickerLedB.Color;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.LedB = value; }, oldValue, newValue));
    }

    private void ColorPickerColorC_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.ColorC;
        uint newValue = ColorPickerColorC.Color;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.ColorC = value; }, oldValue, newValue));
    }

    private void ColorPickerLedC_OnColorPickFinished(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not ConsoleColor consoleColor) return;

        uint oldValue = consoleColor.LedC;
        uint newValue = ColorPickerLedC.Color;

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<uint>(value => { consoleColor.LedC = value; }, oldValue, newValue));
    }


    private void ColorPickerColorA_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        
        blockEvents = true;
        
        TextBoxColorA.Text = $"{ColorPickerColorA.Color - 0xFF000000:X6}";
        
        blockEvents = false;
    }

    private void ColorPickerLedA_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        
        blockEvents = true;

        TextBoxLedA.Text = $"{ColorPickerLedA.Color - 0xFF000000:X6}";
        
        blockEvents = false;
    }

    private void ColorPickerColorB_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        
        blockEvents = true;

        TextBoxColorB.Text = $"{ColorPickerColorB.Color - 0xFF000000:X6}";
        
        blockEvents = false;
    }

    private void ColorPickerLedB_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        
        blockEvents = true;

        TextBoxLedB.Text = $"{ColorPickerLedB.Color - 0xFF000000:X6}";

        blockEvents = false;
    }

    private void ColorPickerColorC_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        
        blockEvents = true;

        TextBoxColorC.Text = $"{ColorPickerColorC.Color - 0xFF000000:X6}";

        blockEvents = false;
    }

    private void ColorPickerLedC_OnColorChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        
        blockEvents = true;

        TextBoxLedC.Text = $"{ColorPickerLedC.Color - 0xFF000000:X6}";

        blockEvents = false;
    }
#endregion UI Event Handlers
}