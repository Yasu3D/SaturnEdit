using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.PrimitiveOperations;

namespace SaturnEdit.Controls;

public partial class NavigatorDialogueVariantCollectionItem : UserControl
{
    public NavigatorDialogueVariantCollectionItem()
    {
        InitializeComponent();
    }

    public NavigatorDialogueVariantCollection? NavigatorDialogueVariantCollection { get; private set; } = null;
    public string? Key { get; set; } = null;

    private bool blockEvents = false;

#region Methods
    public void SetItem(NavigatorDialogueVariantCollection item, string key)
    {
        NavigatorDialogueVariantCollection = item;
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
        if (NavigatorDialogueVariantCollection == null) return;
        if (Key == null) return;
        if (CosmeticSystem.SelectedNavigatorDialogueLanguage == null) return;

        string oldValue = Key;
        string newValue = TextBoxDialogueKey.Text ?? "";
        
        if (oldValue == newValue) return;

        DictionaryRemoveOperation<string, NavigatorDialogueVariantCollection> op0 = new(() => CosmeticSystem.SelectedNavigatorDialogueLanguage.Dialogues, oldValue, NavigatorDialogueVariantCollection);
        DictionaryAddOperation<string, NavigatorDialogueVariantCollection> op1 = new(() => CosmeticSystem.SelectedNavigatorDialogueLanguage.Dialogues, newValue, NavigatorDialogueVariantCollection);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
#endregion UI Event Handlers
}