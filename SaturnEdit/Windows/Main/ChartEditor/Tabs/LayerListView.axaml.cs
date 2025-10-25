using System;
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

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

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
    private void MoveLayerUp()
    {
        if (ListBoxLayers.SelectedItem is not LayerListItem item) return;

        int indexA = ChartSystem.Chart.Layers.IndexOf(item.Layer);
        if (indexA == -1) return;

        int indexB = indexA - 1;
        if (indexB < 0) return;

        Layer layerA = ChartSystem.Chart.Layers[indexA];
        Layer layerB = ChartSystem.Chart.Layers[indexB];

        UndoRedoSystem.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
    }

    private void MoveLayerDown()
    {
        if (ListBoxLayers.SelectedItem is not LayerListItem item) return;

        int indexA = ChartSystem.Chart.Layers.IndexOf(item.Layer);
        if (indexA == -1) return;

        int indexB = indexA + 1;
        if (indexB >= ChartSystem.Chart.Layers.Count) return;

        Layer layerA = ChartSystem.Chart.Layers[indexA];
        Layer layerB = ChartSystem.Chart.Layers[indexB];

        UndoRedoSystem.Push(new LayerSwapOperation(layerA, layerB, indexA, indexB));
    }
    
    private void DeleteLayer()
    {
        if (ListBoxLayers.SelectedItem is not LayerListItem item) return;

        int index = ChartSystem.Chart.Layers.IndexOf(item.Layer);
        
        Layer? newSelection = null;

        if (ChartSystem.Chart.Layers.Count > 1)
        {
            newSelection = index == 0
                ? ChartSystem.Chart.Layers[1]
                : ChartSystem.Chart.Layers[index - 1];
        }

        LayerDeleteOperation op0 = new(item.Layer, index);
        LayerSelectionOperation op1 = new(item.Layer, newSelection);

        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
    
    private void AddLayer()
    {
        Layer layer = new("New Layer");
        int index = ChartSystem.Chart.Layers.Count;

        LayerAddOperation op0 = new(layer, index);
        LayerSelectionOperation op1 = new(SelectionSystem.SelectedLayer, layer); 
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1]));
    }
#endregion Methods

#region System Event Delegates
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
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
            
            TextBlockShortcutMoveItemUp3.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
            TextBlockShortcutMoveItemDown3.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
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
        
        UndoRedoSystem.Push(new LayerNameEditOperation(item.Layer, oldName, newName));
    }
    
    private void LayerItem_OnVisibilityChanged(object? sender, EventArgs e)
    {
        if (blockEvents) return;
        if (sender is not LayerListItem item) return;

        bool oldVisibility = item.Layer.Visible;
        bool newVisibility = !item.Layer.Visible;
        if (oldVisibility == newVisibility) return;
        
        UndoRedoSystem.Push(new LayerVisibilityEditOperation(item.Layer, oldVisibility, newVisibility));
    }
    
    private void ListBoxLayers_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxLayers == null) return;
  
        Layer? oldLayer = SelectionSystem.SelectedLayer;
        Layer? newLayer = ListBoxLayers.SelectedItem is LayerListItem item ? item.Layer : null;
        if (oldLayer == newLayer) return;
        
        UndoRedoSystem.Push(new LayerSelectionOperation(oldLayer, newLayer));
    }
    
    private void ListBoxLayers_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (blockEvents) return;
        if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Control));

        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"]))
        {
            DeleteLayer();
            e.Handled = true;
        }
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"]))
        {
            MoveLayerUp();
            e.Handled = true;
        }
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"]))
        {
            MoveLayerDown();
            e.Handled = true;
        }
    }
    
    private void ButtonMoveItemUp_Click(object? sender, RoutedEventArgs e) => MoveLayerUp();

    private void ButtonMoveItemDown_Click(object? sender, RoutedEventArgs e) => MoveLayerDown();

    private void ButtonDeleteSelection_Click(object? sender, RoutedEventArgs e) => DeleteLayer();

    private void ButtonAddNew_Click(object? sender, RoutedEventArgs e) => AddLayer();
#endregion UI Event Delegates
}