using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Editing;

namespace SaturnEdit.Utilities;

public static class KeyDownBlacklist
{
    public static bool IsInvalidFocusedElement(IInputElement? element)
    {
        return element is TextBox or TextArea;
    }

    public static bool IsInvalidKey(Key key)
    {
        return key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift;
    }
}