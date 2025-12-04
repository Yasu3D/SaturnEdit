using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnEdit.Utilities;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class EventListView : UserControl
{
    public EventListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        
        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region Methods
    private void UpdateEvents()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            for (int i = 0; i < ChartSystem.Chart.Events.Count; i++)
            {
                Event @event = ChartSystem.Chart.Events[i];
                
                if (i < ListBoxEvents.Items.Count)
                {
                    // Modify existing item.
                    if (ListBoxEvents.Items[i] is not EventListItem item) continue;

                    item.SetEvent(@event);
                }
                else
                {
                    // Create new item.
                    EventListItem item = new();
                    item.SetEvent(@event);
                    
                    ListBoxEvents.Items.Add(item);
                }
            }
            
            // Delete redundant items.
            for (int i = ListBoxEvents.Items.Count - 1; i >= ChartSystem.Chart.Events.Count; i--)
            {
                if (ListBoxEvents.Items[i] is not EventListItem item) continue;
                
                ListBoxEvents.Items.Remove(item);
            }
            
            // Set selection.
            if (ListBoxEvents.SelectedItems != null)
            {
                ListBoxEvents.SelectedItems.Clear();
                
                for (int i = 0; i < ListBoxEvents.Items.Count; i++)
                {
                    if (ListBoxEvents.Items[i] is not EventListItem item) continue;
                    if (item.Event == null) continue;
                    if (!SelectionSystem.SelectedObjects.Contains(item.Event!)) continue;
 
                    ListBoxEvents.SelectedItems.Add(item);
                }
            }
            
            blockEvents = false;
        });
    }
#endregion Methods
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockShortcutDeleteSelection.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();

            MenuItemAddTempoChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TempoChange"].ToKeyGesture();
            MenuItemAddMetreChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.MetreChange"].ToKeyGesture();
            MenuItemAddTutorialMarker.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TutorialMarker"].ToKeyGesture();
        });
    }

    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        UpdateEvents();
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        UndoRedoSystem.ChartBranch.OperationHistoryChanged -= ChartBranch_OnOperationHistoryChanged;
        
        base.OnUnloaded(e);
    }
    
    private void ListBoxEvents_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxEvents?.SelectedItems == null) return;

        List<IOperation> operations = [];
        
        foreach (object? obj in e.AddedItems)
        {
            if (obj is not EventListItem item) continue;
            if (item.Event == null) continue;

            operations.Add(new SelectionAddOperation(item.Event, SelectionSystem.LastSelectedObject));
        }

        foreach (object? obj in e.RemovedItems)
        {
            if (obj is not EventListItem item) continue;
            if (item.Event == null) continue;

            operations.Add(new SelectionRemoveOperation(item.Event, SelectionSystem.LastSelectedObject));
        }

        UndoRedoSystem.ChartBranch.Push(new CompositeOperation(operations));
    }

    private void ListBoxEvents_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (blockEvents) return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
        if (SelectionSystem.SelectedLayer == null) return;
        
        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;
            
            TimeSystem.SeekTime(@event.Timestamp.Time, TimeSystem.Division);
            return;
        }
    }

    private void ListBoxEvents_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (blockEvents) return;

        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;

        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"]))
        {
            EditorSystem.EventList_DeleteGlobalEvents();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TempoChange"]))
        {
            EditorSystem.Insert_AddTempoChange(120.0f);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.MetreChange"]))
        {
            EditorSystem.Insert_AddMetreChange(4, 4);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.TutorialMarker"]))
        {
            EditorSystem.Insert_AddTutorialMarker("KEY");
            e.Handled = true;
        }
    }
    
    private void ButtonDeleteEvent_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.EventList_DeleteGlobalEvents();

    private void MenuItemAddTempoChange_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddTempoChange(120.0f);

    private void MenuItemAddMetreChange_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddMetreChange(4, 4);

    private void MenuItemAddTutorialMarker_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddTutorialMarker("KEY");
#endregion UI Event Handlers
}