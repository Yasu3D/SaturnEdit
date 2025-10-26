using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class InspectorView : UserControl
{
    public InspectorView()
    {
        InitializeComponent();

        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
    }

    private bool blockEvents = false;
    
#region System Event Delegates
    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        if (SelectionSystem.SelectedObjects.Count == 0)
        {
            Dispatcher.UIThread.Post(() =>
            {
                blockEvents = true;

                GroupGeneral.IsVisible = false;
                GroupLayers.IsVisible = false;
                GroupShape.IsVisible = false;
                GroupJudgement.IsVisible = false;
                GroupHold.IsVisible = false;
                GroupLaneToggles.IsVisible = false;
                
                blockEvents = false;
            });

            return;
        }
        
        // TODO: Merge these all into one loop.
        
        bool showLayers = false;
        foreach (ITimeable timeable in SelectionSystem.SelectedObjects)
        {
            if (timeable is TempoChangeEvent) continue;
            if (timeable is MetreChangeEvent) continue;
            if (timeable is TutorialMarkerEvent) continue;
            if (timeable is ILaneToggle) continue;
            if (timeable is Bookmark) continue;

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                if (timeable is Event @event && !layer.Events.Contains(@event)) continue;
                if (timeable is Note note && !layer.Notes.Contains(note)) continue;

                showLayers = true;
                break;
            }
        }

        bool showShape = false;
        foreach (ITimeable timeable in SelectionSystem.SelectedObjects)
        {
            if (timeable is not IPositionable) continue;

            showShape = true;
            break;
        }
        
        bool showJudgement = false;
        foreach (ITimeable timeable in SelectionSystem.SelectedObjects)
        {
            if (timeable is not IPlayable) continue;

            showJudgement = true;
            break;
        }
        
        bool showHold = false;
        foreach (ITimeable timeable in SelectionSystem.SelectedObjects)
        {
            if (timeable is not HoldPointNote) continue;

            showHold = true;
            break;
        }
        
        bool showLaneToggles = false;
        foreach (ITimeable timeable in SelectionSystem.SelectedObjects)
        {
            if (timeable is not ILaneToggle) continue;

            showLaneToggles = true;
            break;
        }
        
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;

            GroupGeneral.IsVisible = true;
            GroupLayers.IsVisible = showLayers;
            GroupShape.IsVisible = showShape;
            GroupJudgement.IsVisible = showJudgement;
            GroupHold.IsVisible = showHold;
            GroupLaneToggles.IsVisible = showLaneToggles;
            
            // Update Layer List
            if (ChartSystem.Chart.Layers.Count == 0)
            {
                ComboBoxLayers.IsEnabled = false;
            }
            else
            {
                ComboBoxLayers.IsEnabled = true;
                
                for (int i = 0; i < ChartSystem.Chart.Layers.Count; i++)
                {
                    Layer layer = ChartSystem.Chart.Layers[i];
                
                    if (i < ComboBoxLayers.Items.Count)
                    {
                        // Modify existing item.
                        if (ComboBoxLayers.Items[i] is not ComboBoxItem item) continue;

                        item.Content = layer.Name;
                    }
                    else
                    {
                        // Create new item.
                        ComboBoxItem item = new()
                        {
                            Content = layer.Name,
                        };
                    
                        ComboBoxLayers.Items.Add(item);
                    }
                }
            
                // Delete redundant items.
                for (int i = ComboBoxLayers.Items.Count - 1; i >= ChartSystem.Chart.Layers.Count; i--)
                {
                    ComboBoxLayers.Items.Remove(ComboBoxLayers.Items[i]);
                }
            }
            ComboBoxLayers.SelectedItem = null;
            
            blockEvents = false;
        });
    }
#endregion System Event Delegates

#region UI Event Delegates
    
#endregion UI Event Delegates
}