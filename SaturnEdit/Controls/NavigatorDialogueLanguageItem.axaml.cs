using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Content.Cosmetics;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Controls;

public partial class NavigatorDialogueLanguageItem : UserControl
{
    public NavigatorDialogueLanguageItem()
    {
        InitializeComponent();
    }

    public NavigatorDialogueLanguage? NavigatorDialogueLanguage { get; private set; } = null;
    public string? Key { get; set; } = null;

    private bool blockEvents = false;

#region Methods
    public void SetItem(NavigatorDialogueLanguage item, string key)
    {
        NavigatorDialogueLanguage = item;
        Key = key;
        
        blockEvents = true;
        
        TextBoxLocaleKey.Text = key;

        blockEvents = false;
    }
#endregion Methods
    
#region UI Event Handlers
    private void TextBoxLocaleKey_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (TextBoxLocaleKey == null) return;
        if (NavigatorDialogueLanguage == null) return;
        if (Key == null) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        string oldValue = Key;
        string newValue = TextBoxLocaleKey.Text ?? "";
        
        if (oldValue == newValue) return;

        DictionaryRemoveOperation<string, NavigatorDialogueLanguage> op0 = new(() => navigator.DialogueLanguages, oldValue, NavigatorDialogueLanguage);
        DictionaryAddOperation<string, NavigatorDialogueLanguage> op1 = new(() => navigator.DialogueLanguages, newValue, NavigatorDialogueLanguage);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
#endregion UI Event Handlers
}