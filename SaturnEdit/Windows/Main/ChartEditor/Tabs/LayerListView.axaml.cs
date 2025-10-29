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
using SaturnEdit.UndoRedo.EventOperations;
using SaturnEdit.UndoRedo.LayerOperations;
using SaturnEdit.UndoRedo.SelectionOperations;
using SaturnEdit.Utilities;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

// TODO: Implement moving events and adding new events
public partial class LayerListView : UserControl
{
    public LayerListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
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
    
    private static void MoveLayerUp()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        int indexA = ChartSystem.Chart.Layers.IndexOf(SelectionSystem.SelectedLayer);
        if (indexA == -1) return;

        int indexB = indexA - 1;
        if (indexB < 0) return;

        Layer layerA = ChartSystem.Chart.Layers[indexA];
        Layer layerB = ChartSystem.Chart.Layers[indexB];

        UndoRedoSystem.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
    }

    private static void MoveLayerDown()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        int indexA = ChartSystem.Chart.Layers.IndexOf(SelectionSystem.SelectedLayer);
        if (indexA == -1) return;

        int indexB = indexA + 1;
        if (indexB >= ChartSystem.Chart.Layers.Count) return;

        Layer layerA = ChartSystem.Chart.Layers[indexA];
        Layer layerB = ChartSystem.Chart.Layers[indexB];

        UndoRedoSystem.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
    }
    
    private static void DeleteLayer()
    {
        if (SelectionSystem.SelectedLayer == null) return;

        int index = ChartSystem.Chart.Layers.IndexOf(SelectionSystem.SelectedLayer);
        
        Layer? newSelection = null;

        if (ChartSystem.Chart.Layers.Count > 1)
        {
            newSelection = index == 0
                ? ChartSystem.Chart.Layers[1]
                : ChartSystem.Chart.Layers[index - 1];
        }

        LayerRemoveOperation op0 = new(SelectionSystem.SelectedLayer, index);
        LayerSelectOperation op1 = new(SelectionSystem.SelectedLayer, newSelection);

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
    
    private static void AddLayer()
    {
        Layer layer = ChartSystem.Chart.Layers.Count == 0 
            ? new("Main Layer") 
            : new("New Layer");
        
        int index = ChartSystem.Chart.Layers.Count;

        LayerAddOperation op0 = new(layer, index);
        LayerSelectOperation op1 = new(SelectionSystem.SelectedLayer, layer); 
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }

    private static void MoveEventUp()
    {
        
    }

    private static void MoveEventDown()
    {
        
    }

    private static void DeleteEvent()
    {
        if (SelectionSystem.SelectedLayer == null) return;
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        
        foreach (Event @event in SelectionSystem.SelectedLayer.Events)
        {
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

            int index = SelectionSystem.SelectedLayer.Events.IndexOf(@event);
            
            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new EventRemoveOperation(SelectionSystem.SelectedLayer, @event, index));
        }

        operations.Add(new BuildChartOperation());
        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    private static void AddSpeedChange()
    {
        
    }

    private static void AddVisibilityChange()
    {
        
    }

    private static void AddReverseEffect()
    {
        
    }

    private static void AddStopEffect()
    {
        
    }
#endregion Methods

#region System Event Delegates
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
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
            
            TextBlockShortcutMoveItemUp2.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
            TextBlockShortcutMoveItemDown2.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
            TextBlockShortcutDeleteSelection2.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
    private void LayerItem_OnNameChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not LayerListItem item) return;
        
        string oldName = item.Layer.Name;
        string newName = item.TextBoxLayerName.Text ?? "Unnamed Layer";
        if (oldName == newName) return;
        
        UndoRedoSystem.Push(new LayerRenameOperation(item.Layer, oldName, newName));
    }
    
    private void LayerItem_OnVisibilityChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not LayerListItem item) return;

        bool oldVisibility = item.Layer.Visible;
        bool newVisibility = !item.Layer.Visible;
        if (oldVisibility == newVisibility) return;
        
        UndoRedoSystem.Push(new LayerShowHideOperation(item.Layer, oldVisibility, newVisibility));
    }
    
    private void ListBoxLayers_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLayers == null) return;
  
        Layer? oldLayer = SelectionSystem.SelectedLayer;
        Layer? newLayer = ListBoxLayers.SelectedItem is LayerListItem item ? item.Layer : null;
        if (oldLayer == newLayer) return;
        
        UndoRedoSystem.Push(new LayerSelectOperation(oldLayer, newLayer));
    }
    
    private void ListBoxLayers_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (blockEvents) return;

        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"]))
        {
            DeleteLayer();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"]))
        {
            MoveLayerUp();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"]))
        {
            MoveLayerDown();
            e.Handled = true;
        }
    }
    
    private void ButtonMoveLayerUp_Click(object? sender, RoutedEventArgs e) => MoveLayerUp();

    private void ButtonMoveLayerDown_Click(object? sender, RoutedEventArgs e) => MoveLayerDown();

    private void ButtonDeleteLayer_Click(object? sender, RoutedEventArgs e) => DeleteLayer();

    private void ButtonAddNewLayer_Click(object? sender, RoutedEventArgs e) => AddLayer();
    
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

        UndoRedoSystem.Push(new CompositeOperation(operations));
    }
    
    private void ListBoxEvents_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (blockEvents) return;

        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"]))
        {
            DeleteEvent();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"]))
        {
            MoveEventUp();
            e.Handled = true;
        }
        
        else if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"]))
        {
            MoveEventDown();
            e.Handled = true;
        }
    }

    private void ButtonMoveEventUp_OnClick(object? sender, RoutedEventArgs e) => MoveEventUp();

    private void ButtonMoveEventDown_OnClick(object? sender, RoutedEventArgs e) => MoveEventDown();

    private void ButtonDeleteEvent_OnClick(object? sender, RoutedEventArgs e) => DeleteEvent();

    private void MenuItemAddSpeedChange_OnClick(object? sender, RoutedEventArgs e) => AddSpeedChange();

    private void MenuItemAddVisibilityChange_OnClick(object? sender, RoutedEventArgs e) => AddVisibilityChange();

    private void MenuItemAddReverseEffect_OnClick(object? sender, RoutedEventArgs e) => AddReverseEffect();

    private void MenuItemAddStopEffect_OnClick(object? sender, RoutedEventArgs e) => AddStopEffect();
#endregion UI Event Delegates
}