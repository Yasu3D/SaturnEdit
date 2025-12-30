using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class CosmeticItemEditorView : UserControl
{
    public CosmeticItemEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            TextBoxCosmeticId.Text = CosmeticSystem.CosmeticItem.Id;
            TextBoxCosmeticName.Text = CosmeticSystem.CosmeticItem.Name;
            TextBoxCosmeticDescription.Text = CosmeticSystem.CosmeticItem.Description;
            TextBoxCosmeticAuthor.Text = CosmeticSystem.CosmeticItem.Author;
            TextBoxCosmeticRarity.Text = CosmeticSystem.CosmeticItem.Rarity.ToString(CultureInfo.InvariantCulture);
            
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
    private void TextBoxCosmeticId_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticId == null) return;
        
        string oldId = CosmeticSystem.CosmeticItem.Id;
        string newId = TextBoxCosmeticId.Text ?? "";
        if (oldId == newId) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { CosmeticSystem.CosmeticItem.Id = value; }, oldId, newId));
    }

    private void ButtonRegenerateCosmeticId_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;

        string oldId = CosmeticSystem.CosmeticItem.Id;
        string newId = Guid.NewGuid().ToString();
        if (oldId == newId) return;

        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { CosmeticSystem.CosmeticItem.Id = value; }, oldId, newId));
    }

    private void TextBoxCosmeticName_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticName == null) return;

        string oldName = CosmeticSystem.CosmeticItem.Name;
        string newName = TextBoxCosmeticName.Text ?? "";
        if (oldName == newName) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { CosmeticSystem.CosmeticItem.Name = value; }, oldName, newName));
    }
    
    private void TextBoxCosmeticDescription_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticDescription == null) return;

        string oldDescription = CosmeticSystem.CosmeticItem.Description;
        string newDescription = TextBoxCosmeticDescription.Text ?? "";
        if (oldDescription == newDescription) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { CosmeticSystem.CosmeticItem.Description = value; }, oldDescription, newDescription));
    }

    private void TextBoxCosmeticAuthor_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxCosmeticAuthor == null) return;

        string oldAuthor = CosmeticSystem.CosmeticItem.Author;
        string newAuthor = TextBoxCosmeticAuthor.Text ?? "";
        if (oldAuthor == newAuthor) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { CosmeticSystem.CosmeticItem.Author = value; }, oldAuthor, newAuthor));
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
        
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<int>(value => { CosmeticSystem.CosmeticItem.Rarity = value; }, oldRarity, newRarity));
        }
        catch (Exception ex)
        {
            // Reset Value
            UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<int>(value => { CosmeticSystem.CosmeticItem.Rarity = value; }, CosmeticSystem.CosmeticItem.Rarity, 0));

            if (ex is not (FormatException or OverflowException))
            {
                LoggingSystem.WriteSessionLog(ex.ToString());
            }
        }
    }
#endregion UI Event Handlers
}