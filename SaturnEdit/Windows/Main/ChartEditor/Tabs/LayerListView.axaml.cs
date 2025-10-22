using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SaturnData.Notation.Core;
using SaturnEdit.Controls;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class LayerListView : UserControl
{
    public LayerListView()
    {
        InitializeComponent();

        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        SelectionSystem.SelectionChanged += OnSelectionChanged;
        SelectionSystem.LayerChanged += OnLayerChanged;
        ChartSystem.ChartChanged += OnChartChanged;
        
        RefreshLayers();
    }

    private void OnChartChanged(object? sender, EventArgs e)
    {
        RefreshLayers();
    }

    private void OnLayerChanged(object? sender, EventArgs e)
    {
        RefreshLayers();
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
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

    private void OnNameChanged(object? sender, EventArgs e)
    {
        if (sender is not LayerListItem item) return;

        item.Layer.Name = string.IsNullOrWhiteSpace(item.TextBoxLayerName.Text) 
            ? "Unnamed Layer" 
            : item.TextBoxLayerName.Text;
        
        ChartSystem.InvokeChartChanged();
    }
    
    private void OnVisibilityChanged(object? sender, EventArgs e)
    {
        if (sender is not LayerListItem item) return;

        item.Layer.Visible = !item.Layer.Visible;
        
        ChartSystem.InvokeChartChanged();
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
                LayerListItem item = new(layer);
                item.NameChanged += OnNameChanged;
                item.VisibilityChanged += OnVisibilityChanged;
                
                ListBoxLayers.Items.Add(item);
            }
        }
        
        // Delete redundant items.
        for (int i = ListBoxLayers.Items.Count - 1; i >= ChartSystem.Chart.Layers.Count; i--)
        {
            if (ListBoxLayers.Items[i] is not LayerListItem item) continue;

            item.NameChanged -= OnNameChanged;
            item.VisibilityChanged -= OnVisibilityChanged;
            
            ListBoxLayers.Items.Remove(item);
        }
    }
}