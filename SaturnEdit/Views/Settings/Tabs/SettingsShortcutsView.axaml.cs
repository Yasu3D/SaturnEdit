using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
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
        InitializeList(SettingsSystem.ShortcutSettings.Shortcuts.ToList());
    }

    public void InitializeList(List<KeyValuePair<string, Shortcut>> shortcuts)
    {
        for (int i = 0; i < shortcuts.Count; i++)
        {
            if (i < ListBoxShortcuts.Items.Count)
            {
                // Modify existing items
                if (ListBoxShortcuts.Items[i] is not ShortcutListItem item) continue;
                
                item.Action = shortcuts[i].Key;
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
                ShortcutListItem newItem = new() { Action = shortcuts[i].Key, };
                
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