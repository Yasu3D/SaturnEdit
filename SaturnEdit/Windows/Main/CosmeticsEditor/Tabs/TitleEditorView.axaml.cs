using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.PrimitiveOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class TitleEditorView : UserControl
{
    public TitleEditorView()
    {
        InitializeComponent();
        
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    private void CosmeticBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (CosmeticSystem.CosmeticItem is not Title title) return;
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            TextBoxMessage.Text = title.Message;
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers
    
#region UI Event Handlers
    private void TextBoxMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Title title) return;

        string oldValue = title.Message;
        string newValue = TextBoxMessage.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new StringEditOperation(value => { title.Message = value; }, oldValue, newValue));
    }
#endregion UI Event Handlers
}