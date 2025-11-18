using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.LayerOperations;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnEdit.Utilities;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class LayerListView : UserControl
{
    public LayerListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.ChartBranch.OperationHistoryChanged += ChartBranch_OnOperationHistoryChanged;
        ChartBranch_OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region Methods
    private void UpdateLayers()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
            {
                Layer layer = ChartSystem.Chart.Layers[i];
                
                if (i < ListBoxLayers.Items.Count)
                {
                    // Modify existing item.
                    if (ListBoxLayers.Items[i] is not LayerListItem item) continue;

                    item.SetLayer(layer);
                }
                else
                {
                    // Create new item.
                    LayerListItem item = new();
                    item.SetLayer(layer);
                    
                    item.NameChanged += LayerItem_OnNameChanged;
                    item.VisibilityChanged += LayerItem_OnVisibilityChanged;
                    
                    ListBoxLayers.Items.Add(item);
                }
            }
            
            // Delete redundant items.
            for (int i = ListBoxLayers.Items.Count - 1; i >= ChartSystem.Chart.Layers.Count; i--)
            {
                if (ListBoxLayers.Items[i] is not LayerListItem item) continue;

                item.NameChanged -= LayerItem_OnNameChanged;
                item.VisibilityChanged -= LayerItem_OnVisibilityChanged;
                
                ListBoxLayers.Items.Remove(item);
            }
            
            // Set selection.
            ListBoxLayers.SelectedItem = ListBoxLayers.Items.FirstOrDefault(x => x is LayerListItem item && item.Layer == SelectionSystem.SelectedLayer);
            
            blockEvents = false;
        });
    }

    private void UpdateEvents()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (SelectionSystem.SelectedLayer == null)
            {
                blockEvents = true;
                
                ListBoxEvents.Items.Clear();

                blockEvents = false;
                return;
            }
            
            blockEvents = true;
            
            for (int i = 0; i < SelectionSystem.SelectedLayer.Events.Count; i++)
            {
                Event @event = SelectionSystem.SelectedLayer.Events[i];
                
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
            for (int i = ListBoxEvents.Items.Count - 1; i >= SelectionSystem.SelectedLayer.Events.Count; i--)
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
    private void ChartBranch_OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        UpdateLayers();
        UpdateEvents();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBlockShortcutMoveItemUp1.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
            TextBlockShortcutMoveItemDown1.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
            TextBlockShortcutDeleteSelection1.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
            TextBlockShortcutDeleteSelection2.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();

            MenuItemAddSpeedChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.SpeedChange"].ToKeyGesture();
            MenuItemAddVisibilityChange.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.VisibilityChange"].ToKeyGesture();
            MenuItemAddStopEffect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.StopEffect"].ToKeyGesture();
            MenuItemAddReverseEffect.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.ReverseEffect"].ToKeyGesture();
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    private void LayerItem_OnNameChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not LayerListItem item) return;
        
        string oldName = item.Layer.Name;
        string newName = item.TextBoxLayerName.Text ?? "Unnamed Layer";
        if (oldName == newName) return;
        
        UndoRedoSystem.ChartBranch.Push(new LayerRenameOperation(item.Layer, oldName, newName));
    }
    
    private void LayerItem_OnVisibilityChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not LayerListItem item) return;

        bool oldVisibility = item.Layer.Visible;
        bool newVisibility = !item.Layer.Visible;
        if (oldVisibility == newVisibility) return;
        
        UndoRedoSystem.ChartBranch.Push(new LayerShowHideOperation(item.Layer, oldVisibility, newVisibility));
    }
    
    private void ListBoxLayers_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLayers == null) return;
  
        Layer? oldLayer = SelectionSystem.SelectedLayer;
        Layer? newLayer = ListBoxLayers.SelectedItem is LayerListItem item ? item.Layer : null;
        if (oldLayer == newLayer) return;
        
        UndoRedoSystem.ChartBranch.Push(new LayerSelectOperation(oldLayer, newLayer));
    }
    
    private void ListBoxLayers_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (blockEvents) return;

        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"]))
        {
            EditorSystem.LayerList_DeleteLayer();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"]))
        {
            EditorSystem.LayerList_MoveLayerUp();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"]))
        {
            EditorSystem.LayerList_MoveLayerDown();
            e.Handled = true;
        }
    }
    
    private void ButtonMoveLayerUp_Click(object? sender, RoutedEventArgs e) => EditorSystem.LayerList_MoveLayerUp();

    private void ButtonMoveLayerDown_Click(object? sender, RoutedEventArgs e) => EditorSystem.LayerList_MoveLayerDown();

    private void ButtonDeleteLayer_Click(object? sender, RoutedEventArgs e) => EditorSystem.LayerList_DeleteLayer();

    private void ButtonAddNewLayer_Click(object? sender, RoutedEventArgs e) => EditorSystem.LayerList_AddLayer();
    
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
            EditorSystem.LayerList_DeleteEvents();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.SpeedChange"]))
        {
            EditorSystem.Insert_AddSpeedChange(1.0f);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.VisibilityChange"]))
        {
            EditorSystem.Insert_AddVisibilityChange(true);
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.StopEffect"]))
        {
            EditorSystem.Insert_AddStopEffect();
            e.Handled = true;
        }
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Insert.ReverseEffect"]))
        {
            EditorSystem.Insert_AddReverseEffect();
            e.Handled = true;
        }
    }

    private void ButtonDeleteEvent_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.LayerList_DeleteEvents();

    private void MenuItemAddSpeedChange_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddSpeedChange(1.0f);

    private void MenuItemAddVisibilityChange_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddVisibilityChange(true);

    private void MenuItemAddReverseEffect_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddReverseEffect();

    private void MenuItemAddStopEffect_OnClick(object? sender, RoutedEventArgs e) => EditorSystem.Insert_AddStopEffect();
    
    private void ListBoxEvents_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (blockEvents) return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) return;
        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) return;
        if (SelectionSystem.SelectedLayer == null) return;
        
        foreach (Event @event in SelectionSystem.SelectedLayer.Events)
        {
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;
            
            TimeSystem.SeekTime(@event.Timestamp.Time, TimeSystem.Division);
            return;
        }
    }
#endregion UI Event Handlers
}