using System;
using Avalonia.Controls;
using SaturnData.Notation.Core;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo.Operations;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

// TODO: This is kinda broken rn.
// Make events shut up

public partial class LayerListView : UserControl
{
    public LayerListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        
        RefreshLayers();
    }
    
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        RefreshLayers();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        TextBlockShortcutMoveItemUp1.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown1.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
        TextBlockShortcutDeleteSelection1.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        
        TextBlockShortcutMoveItemUp2.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown2.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
        TextBlockShortcutDeleteSelection2.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        
        TextBlockShortcutMoveItemUp3.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemUp"].ToString();
        TextBlockShortcutMoveItemDown3.Text = SettingsSystem.ShortcutSettings.Shortcuts["List.MoveItemDown"].ToString();
    }

    private void LayerItem_OnNameChanged(object? sender, EventArgs e)
    {
        if (sender is not LayerListItem item) return;
        
        string oldName = item.Layer.Name;
        string newName = item.TextBoxLayerName.Text ?? "Unnamed Layer";
        
        Console.WriteLine($"NameChanged \"{oldName}\" \"{newName}\"");
        
        UndoRedoSystem.Push(new LayerNameEditOperation(item.Layer, oldName, newName));
    }
    
    private void LayerItem_OnVisibilityChanged(object? sender, EventArgs e)
    {
        if (sender is not LayerListItem item) return;

        bool oldVisibility = item.Layer.Visible;
        bool newVisibility = !item.Layer.Visible;
        
        UndoRedoSystem.Push(new LayerVisibilityEditOperation(item.Layer, oldVisibility, newVisibility));
    }
    
    private void RefreshLayers()
    {
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
    }
}