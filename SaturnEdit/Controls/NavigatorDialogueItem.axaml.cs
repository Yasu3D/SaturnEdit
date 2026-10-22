using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Content.Cosmetics.NavigatorData;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Controls;

public partial class NavigatorDialogueItem : UserControl
{
    public NavigatorDialogueItem()
    {
        InitializeComponent();
    }

    public NavigatorDialogue? NavigatorDialogue { get; private set; } = null;
    public string? Key { get; set; } = null;

    private bool blockEvents = false;

#region Methods
    public void SetItem(NavigatorDialogue item, string key)
    {
        NavigatorDialogue = item;
        Key = key;
        
        blockEvents = true;
        
        TextBoxDialogueKey.Text = key;

        blockEvents = false;
    }
#endregion Methods
    
#region UI Event Handlers
    private void TextBoxDialogueKey_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxDialogueKey == null) return;
        if (NavigatorDialogue == null) return;
        if (Key == null) return;
        if (CosmeticSystem.SelectedNavigatorLanguage == null) return;

        string oldValue = Key;
        string newValue = TextBoxDialogueKey.Text ?? "";
        
        if (oldValue == newValue) return;

        DictionaryRemoveOperation<string, NavigatorDialogue> op0 = new(() => CosmeticSystem.SelectedNavigatorLanguage.Dialogues, oldValue, NavigatorDialogue);
        DictionaryAddOperation<string, NavigatorDialogue> op1 = new(() => CosmeticSystem.SelectedNavigatorLanguage.Dialogues, newValue, NavigatorDialogue);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
#endregion UI Event Handlers
}