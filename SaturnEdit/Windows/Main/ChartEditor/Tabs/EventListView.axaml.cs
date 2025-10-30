using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.EventOperations;
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
        
        UndoRedoSystem.OperationHistoryChanged += OnOperationHistoryChanged;
        OnOperationHistoryChanged(null, EventArgs.Empty);
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

    private static void DeleteEvent()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;

        List<IOperation> operations = [];
        
        foreach (Event @event in ChartSystem.Chart.Events)
        {
            if (!SelectionSystem.SelectedObjects.Contains(@event)) continue;

            int index = ChartSystem.Chart.Events.IndexOf(@event);
            if (index == -1) continue;
            
            operations.Add(new SelectionRemoveOperation(@event, SelectionSystem.LastSelectedObject));
            operations.Add(new GlobalEventRemoveOperation(@event, index));
        }

        operations.Add(new BuildChartOperation());
        UndoRedoSystem.Push(new CompositeOperation(operations));
    }

    public static void AddTempoChange()
    {
        TempoChangeEvent tempoChangeEvent = new(new(TimeSystem.Timestamp.FullTick), 120.000000f);
        int index = ChartSystem.Chart.Events.FindLastIndex(x => x.Timestamp.FullTick <= tempoChangeEvent.Timestamp.FullTick);

        GlobalEventAddOperation op0 = new(tempoChangeEvent, index);
        SelectionAddOperation op1 = new(tempoChangeEvent, SelectionSystem.LastSelectedObject);
        BuildChartOperation op2 = new();
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1, op2]));
    }

    public static void AddMetreChange()
    {
        MetreChangeEvent metreChangeEvent = new(new(TimeSystem.Timestamp.FullTick), 4, 4);
        int index = ChartSystem.Chart.Events.FindLastIndex(x => x.Timestamp.FullTick <= metreChangeEvent.Timestamp.FullTick);

        GlobalEventAddOperation op0 = new(metreChangeEvent, index);
        SelectionAddOperation op1 = new(metreChangeEvent, SelectionSystem.LastSelectedObject);
        BuildChartOperation op2 = new();
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1, op2]));
    }

    public static void AddTutorialMarker()
    {
        TutorialMarkerEvent tutorialMarkerEvent = new(new(TimeSystem.Timestamp.FullTick), "KEY");
        int index = ChartSystem.Chart.Events.FindLastIndex(x => x.Timestamp.FullTick <= tutorialMarkerEvent.Timestamp.FullTick);

        GlobalEventAddOperation op0 = new(tutorialMarkerEvent, index);
        SelectionAddOperation op1 = new(tutorialMarkerEvent, SelectionSystem.LastSelectedObject);
        BuildChartOperation op2 = new();
        
        UndoRedoSystem.Push(new CompositeOperation([op0, op1, op2]));
    }
#endregion Methods
    
#region System Event Delegates
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

    private void OnOperationHistoryChanged(object? sender, EventArgs e)
    {
        UpdateEvents();
    }
#endregion System Event Delegates
    
#region UI Event Delegates
    private void ListBoxEvents_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
    }

    private void ListBoxEvents_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (blockEvents) return;
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
    }
    
    private void ButtonDeleteEvent_OnClick(object? sender, RoutedEventArgs e) => DeleteEvent();

    private void MenuItemAddTempoChange_OnClick(object? sender, RoutedEventArgs e) => AddTempoChange();

    private void MenuItemAddMetreChange_OnClick(object? sender, RoutedEventArgs e) => AddMetreChange();

    private void MenuItemAddTutorialMarker_OnClick(object? sender, RoutedEventArgs e) => AddTutorialMarker();
#endregion UI Event Delegates
}