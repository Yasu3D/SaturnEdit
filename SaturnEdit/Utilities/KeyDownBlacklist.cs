using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Editing;
using SaturnEdit.Windows.Dialogs.Settings.Tabs;

namespace SaturnEdit.Utilities;

public static class KeyDownBlacklist
{
    public static bool IsInvalidState()
    {
        if (SettingsShortcutsView.DefiningShortcut) return true;

        return false;
    }
    
    public static bool IsInvalidFocusedElement(IInputElement? element)
    {
        if (element is TextBox) return true;
        if (element is TextArea) return true;

        return false;
    }

    public static bool IsInvalidKey(Key key)
    {
        return key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift;
    }
}