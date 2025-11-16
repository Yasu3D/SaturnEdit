using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.CosmeticOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class CosmeticItemEditorView : UserControl
{
    public CosmeticItemEditorView()
    {
        InitializeComponent();

        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Delegates
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxCosmeticId.Text = CosmeticSystem.CosmeticItem.Id;
            TextBoxCosmeticName.Text = CosmeticSystem.CosmeticItem.Name;
            TextBoxCosmeticAuthor.Text = CosmeticSystem.CosmeticItem.Author;
            TextBoxCosmeticRarity.Text = CosmeticSystem.CosmeticItem.Rarity.ToString(CultureInfo.InvariantCulture);
            
            blockEvents = false;
        });
    }
#endregion System Event Delegates
    
#region UI Event Delegates
    private void TextBoxCosmeticId_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticId == null) return;
        
        string oldId = CosmeticSystem.CosmeticItem.Id;
        string newId = TextBoxCosmeticId.Text ?? "";
        if (oldId == newId) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new CosmeticItemIdEditOperation(oldId, newId));
    }

    private void ButtonRegenerateCosmeticId_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        string oldId = CosmeticSystem.CosmeticItem.Id;
        string newId = Guid.NewGuid().ToString();
        if (oldId == newId) return;

        UndoRedoSystem.CosmeticBranch.Push(new CosmeticItemIdEditOperation(oldId, newId));
    }

    private void TextBoxCosmeticName_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticName == null) return;

        string oldName = CosmeticSystem.CosmeticItem.Name;
        string newName = TextBoxCosmeticName.Text ?? "";
        if (oldName == newName) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new CosmeticItemNameEditOperation(oldName, newName));
    }

    private void TextBoxCosmeticAuthor_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticAuthor == null) return;

        string oldAuthor = CosmeticSystem.CosmeticItem.Author;
        string newAuthor = TextBoxCosmeticAuthor.Text ?? "";
        if (oldAuthor == newAuthor) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new CosmeticItemAuthorEditOperation(oldAuthor, newAuthor));
    }
    
    private void TextBoxCosmeticRarity_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticRarity == null) return;

        try
        {
            int oldRarity = CosmeticSystem.CosmeticItem.Rarity;
            int newRarity = Convert.ToInt32(TextBoxCosmeticRarity.Text ?? "", CultureInfo.InvariantCulture);
            if (oldRarity == newRarity) return;
        
            UndoRedoSystem.CosmeticBranch.Push(new CosmeticItemRarityEditOperation(oldRarity, newRarity));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new CosmeticItemRarityEditOperation(CosmeticSystem.CosmeticItem.Rarity, 0));

            if (ex is not (FormatException or OverflowException))
            {
                Console.WriteLine(ex);
            }
        }
    }
#endregion UI Event Delegates
}