using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Content.Cosmetics;
using SaturnData.Content.Cosmetics.NavigatorData;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.GenericOperations;

namespace SaturnEdit.Controls;

public partial class NavigatorLanguageItem : UserControl
{
    public NavigatorLanguageItem()
    {
        InitializeComponent();
    }

    public NavigatorLanguage? NavigatorLanguage { get; private set; } = null;
    public string? Key { get; set; } = null;

    private bool blockEvents = false;

#region Methods
    public void SetItem(NavigatorLanguage item, string key)
    {
        NavigatorLanguage = item;
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
        if (NavigatorLanguage == null) return;
        if (Key == null) return;
        if (CosmeticSystem.CosmeticItem is not Navigator navigator) return;

        string oldValue = Key;
        string newValue = TextBoxLocaleKey.Text ?? "";
        
        if (oldValue == newValue) return;

        DictionaryRemoveOperation<string, NavigatorLanguage> op0 = new(() => navigator.Writing.Languages, oldValue, NavigatorLanguage);
        DictionaryAddOperation<string, NavigatorLanguage> op1 = new(() => navigator.Writing.Languages, newValue, NavigatorLanguage);
        
        UndoRedoSystem.CosmeticBranch.Push(new CompositeOperation([op0, op1]));
    }
#endregion UI Event Handlers
}