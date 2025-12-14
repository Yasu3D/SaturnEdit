using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Windows.Main.CosmeticsEditor.Tabs;

public partial class TitleEditorView : UserControl
{
    public TitleEditorView()
    {
        InitializeComponent();
    }

    private bool blockEvents = false;
    
#region System Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged += CosmeticBranch_OnOperationHistoryChanged;
        CosmeticBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
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
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        UndoRedoSystem.CosmeticBranch.OperationHistoryChanged -= CosmeticBranch_OnOperationHistoryChanged;
        
        base.OnUnloaded(e);
    }
    
    private void TextBoxMessage_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (CosmeticSystem.CosmeticItem is not Title title) return;

        string oldValue = title.Message;
        string newValue = TextBoxMessage.Text ?? "";

        if (oldValue == newValue) return;
        
        UndoRedoSystem.CosmeticBranch.Push(new GenericEditOperation<string>(value => { title.Message = value; }, oldValue, newValue));
    }
#endregion UI Event Handlers
}