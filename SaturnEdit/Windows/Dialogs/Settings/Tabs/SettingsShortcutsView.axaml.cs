using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;

namespace SaturnEdit.Windows.Dialogs.Settings.Tabs;

public partial class SettingsShortcutsView : UserControl
{
    public SettingsShortcutsView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        
        ListBoxShortcuts.AddHandler(KeyDownEvent, ListBoxShortcuts_OnKeyDown, RoutingStrategies.Tunnel);
    }

    public static bool DefiningShortcut = false; 
    
    private ShortcutListItem? currentItem = null;
    
#region Methods
    private void StartDefiningShortcut(ShortcutListItem item)
    {
        // stop previous definition if it's still going.
        if (DefiningShortcut && currentItem != null)
        {
            StopDefiningShortcut();
        }
        
        DefiningShortcut = true;
        currentItem = item;
        
        currentItem.TextBlockShortcut.IsVisible = false;
        currentItem.TextBlockWaitingForInput.IsVisible = true;
    }

    public void StopDefiningShortcut()
    {
        // reset previous item
        if (currentItem != null)
        {
            currentItem.TextBlockShortcut.IsVisible = true;
            currentItem.TextBlockWaitingForInput.IsVisible = false;
        }
        
        DefiningShortcut = false;
        currentItem = null;
    }

    private void ClearShortcut(ShortcutListItem item)
    {
        if (DefiningShortcut)
        {
            StopDefiningShortcut();
        }
        
        SettingsSystem.ShortcutSettings.SetShortcut(item.Key, new(Key.None, false, false, false, item.Shortcut.GroupMessage, item.Shortcut.ActionMessage));
    }

    private void ResetShortcut(ShortcutListItem item)
    {
        if (DefiningShortcut)
        {
            StopDefiningShortcut();
        }

        // Very primitive and dumb solution, but it works (:
        ShortcutSettings tempShortcutSettings = new();
        SettingsSystem.ShortcutSettings.SetShortcut(item.Key, tempShortcutSettings.Shortcuts[item.Key]);
    }
    
    private void GenerateList(string query)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Application.Current == null) return;
        
            if (DefiningShortcut)
            {
                StopDefiningShortcut();
            }
        
            string[] queryParts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
            List<KeyValuePair<string, Shortcut>> shortcuts = query == ""
                ? SettingsSystem.ShortcutSettings.Shortcuts.ToList()
                : SettingsSystem.ShortcutSettings.Shortcuts.Where(x =>
                {
                    foreach (string queryPart in queryParts)
                    {
                        bool group = Application.Current.TryGetResource(x.Value.GroupMessage, Application.Current.ActualThemeVariant, out object? groupResource)
                                     && groupResource is string groupName
                                     && groupName.Contains(queryPart, StringComparison.OrdinalIgnoreCase);
                    
                        bool action = Application.Current.TryGetResource(x.Value.ActionMessage, Application.Current.ActualThemeVariant, out object? actionResource)
                                      && actionResource is string actionName
                                      && actionName.Contains(queryPart, StringComparison.OrdinalIgnoreCase);

                        bool shortcut = x.Value.ToString().Contains(queryPart, StringComparison.OrdinalIgnoreCase);

                        if (group || action || shortcut)
                        {
                            return true;
                        }
                    }
                
                    return false;
                }).ToList();    
        
            for (int i = 0; i < shortcuts.Count; i++)
            {
                if (i < ListBoxShortcuts.Items.Count)
                {
                    // Modify existing items
                    if (ListBoxShortcuts.Items[i] is not ShortcutListItem item) continue;
                
                    item.Key = shortcuts[i].Key;
                    item.Shortcut = shortcuts[i].Value;
                    item.TextBlockGroup.Bind(TextBlock.TextProperty, new DynamicResourceExtension(shortcuts[i].Value.GroupMessage));
                    item.TextBlockAction.Bind(TextBlock.TextProperty, new DynamicResourceExtension(shortcuts[i].Value.ActionMessage));
                    item.TextBlockShortcut.Text = shortcuts[i].Value.ToString();

                    if (i % 2 != 0)
                    {
                        item.Bind(BackgroundProperty, new DynamicResourceExtension("ButtonSecondaryPointerOver"));
                    }
                    else
                    {
                        item.Background = new SolidColorBrush(Colors.Transparent);
                    }
                }
                else
                {
                    // Create new item
                    ShortcutListItem newItem = new()
                    {
                        Key = shortcuts[i].Key,
                        Shortcut = shortcuts[i].Value,
                    };
                
                    newItem.TextBlockGroup.Bind(TextBlock.TextProperty, new DynamicResourceExtension(newItem.Shortcut.GroupMessage));
                    newItem.TextBlockAction.Bind(TextBlock.TextProperty, new DynamicResourceExtension(newItem.Shortcut.ActionMessage));
                    newItem.TextBlockShortcut.Text = newItem.Shortcut.ToString();

                    if (i % 2 != 0)
                    {
                        newItem.Bind(BackgroundProperty, new DynamicResourceExtension("ButtonSecondaryPointerOver"));
                    }
                    else
                    {
                        newItem.Background = new SolidColorBrush(Colors.Transparent);
                    }
                
                    ListBoxShortcuts.Items.Add(newItem);
                }
            }

            for (int i = ListBoxShortcuts.Items.Count - 1; i >= shortcuts.Count; i--)
            {
                if (ListBoxShortcuts.Items[i] is not ShortcutListItem item) continue;
            
                ListBoxShortcuts.Items.Remove(item);
            }
        });
    }
#endregion Methods

#region System Event Delegates
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        GenerateList(TextBoxSearch?.Text ?? "");
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void ListBoxShortcuts_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!DefiningShortcut) return;
        if (currentItem == null) return;
        
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        
        if (e.Key is Key.Escape)
        {
            StopDefiningShortcut();
            return;
        }
        
        if (e.Key is Key.Up or Key.Down or Key.Left or Key.Right or Key.PageUp or Key.PageDown or Key.End or Key.Home)
        {
            e.Handled = true;
        }

        // Skip modifier keys.
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt) return;

        Shortcut newShortcut = new(
            e.Key,
            e.KeyModifiers.HasFlag(KeyModifiers.Control),
            e.KeyModifiers.HasFlag(KeyModifiers.Alt),
            e.KeyModifiers.HasFlag(KeyModifiers.Shift),
            currentItem.Shortcut.GroupMessage,
            currentItem.Shortcut.ActionMessage);

        SettingsSystem.ShortcutSettings.SetShortcut(currentItem.Key, newShortcut);
        
        StopDefiningShortcut();
    }
    
    private void ListBoxShortcuts_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (ListBoxShortcuts.SelectedItem is not ShortcutListItem item) return;
        if (!item.IsPointerOver) return;

        StartDefiningShortcut(item);
    }
    
    private void ListBoxShortcuts_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsLeftButtonPressed 
            || e.Properties.IsRightButtonPressed 
            || !e.Properties.IsMiddleButtonPressed) return;

        ShortcutListItem? pointerOverItem = null;
        foreach (object? obj in ListBoxShortcuts.Items)
        {
            if (obj is ShortcutListItem item && item.IsPointerOver)
            {
                pointerOverItem = item;
                break;
            }
        }

        if (pointerOverItem != null)
        {
            ClearShortcut(pointerOverItem);   
        }
    }
    
    private void ListBoxShortcuts_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DefiningShortcut)
        {
            StopDefiningShortcut();
        }
    }
    
    private void TextBoxSearch_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        StopDefiningShortcut();
        ListBoxShortcuts.SelectedItem = null;
    }
    
    private void ButtonDefine_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ListBoxShortcuts.SelectedItem is not ShortcutListItem item) return;
        StartDefiningShortcut(item);
    }

    private void ButtonClear_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ListBoxShortcuts.SelectedItem is not ShortcutListItem item) return;
        
        ClearShortcut(item);
    }

    private void ButtonResetToDefault_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ListBoxShortcuts.SelectedItem is not ShortcutListItem item) return;
        ResetShortcut(item);
    }
    
    private void TextBoxSearch_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (TextBoxSearch == null) return;
        GenerateList(TextBoxSearch.Text ?? "");
    }
#endregion UI Event Delegates
}