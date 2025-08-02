using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using SaturnEdit.Controls;
using SaturnEdit.Systems;

namespace SaturnEdit.Views.Settings.Tabs;

public partial class SettingsShortcutsView : UserControl
{
    public SettingsShortcutsView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        GenerateList(TextBoxSearch?.Text ?? "");
    }

    private bool definingShortcut = false; 
    private ShortcutListItem? currentItem = null;

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!definingShortcut) return;
        if (currentItem == null) return;

        if (e.Key is Key.Escape)
        {
            StopDefiningShortcut();
            return;
        }

        // Skip modifier keys.
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt) return;

        Shortcut newShortcut = new(
            e.Key,
            e.KeyModifiers.HasFlag(KeyModifiers.Control),
            e.KeyModifiers.HasFlag(KeyModifiers.Alt),
            e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        SettingsSystem.ShortcutSettings.SetShortcut(currentItem.ActionName, newShortcut);
        
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
        if (definingShortcut)
        {
            StopDefiningShortcut();
        }
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
    
    
    private void StartDefiningShortcut(ShortcutListItem item)
    {
        // stop previous definition if it's still going.
        if (definingShortcut && currentItem != null)
        {
            StopDefiningShortcut();
        }
        
        definingShortcut = true;
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
        
        definingShortcut = false;
        currentItem = null;
    }

    private void ClearShortcut(ShortcutListItem item)
    {
        if (definingShortcut)
        {
            StopDefiningShortcut();
        }

        SettingsSystem.ShortcutSettings.SetShortcut(item.ActionName, Shortcut.None);
    }

    private void ResetShortcut(ShortcutListItem item)
    {
        if (definingShortcut)
        {
            StopDefiningShortcut();
        }

        // Very primitive and dumb solution, but it works (:
        ShortcutSettings tempShortcutSettings = new();
        SettingsSystem.ShortcutSettings.SetShortcut(item.ActionName, tempShortcutSettings.Shortcuts[item.ActionName]);
    }
    
    private void GenerateList(string query)
    {
        if (Application.Current == null) return;
        
        if (definingShortcut)
        {
            StopDefiningShortcut();
        }
        
        List<KeyValuePair<string, Shortcut>> shortcuts = query == ""
            ? SettingsSystem.ShortcutSettings.Shortcuts.ToList()
            : SettingsSystem.ShortcutSettings.Shortcuts.Where(x =>
            {
                bool action = Application.Current.TryGetResource(x.Key, Application.Current.ActualThemeVariant, out object? resource)
                              && resource is string actionName
                              && actionName.Contains(query, StringComparison.OrdinalIgnoreCase);

                bool shortcut = x.Value.ToString().Contains(query, StringComparison.OrdinalIgnoreCase);

                return action || shortcut;
            }).ToList();
        
        for (int i = 0; i < shortcuts.Count; i++)
        {
            if (i < ListBoxShortcuts.Items.Count)
            {
                // Modify existing items
                if (ListBoxShortcuts.Items[i] is not ShortcutListItem item) continue;
                
                item.ActionName = shortcuts[i].Key;
                item.Shortcut = shortcuts[i].Value;
                item.TextBlockAction.Bind(TextBlock.TextProperty, new DynamicResourceExtension(shortcuts[i].Key));
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
                    ActionName = shortcuts[i].Key,
                    Shortcut = shortcuts[i].Value,
                };
                
                newItem.TextBlockAction.Bind(TextBlock.TextProperty, new DynamicResourceExtension(shortcuts[i].Key));
                newItem.TextBlockShortcut.Text = shortcuts[i].Value.ToString();

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
    }
}