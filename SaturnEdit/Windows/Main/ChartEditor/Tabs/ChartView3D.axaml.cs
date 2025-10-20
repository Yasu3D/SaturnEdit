using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnView;
using SkiaSharp;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ChartView3D : UserControl
{
    public ChartView3D()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        ActualThemeVariantChanged += OnActualThemeVariantChanged;
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
    }

    private readonly CanvasInfo canvasInfo = new();
    private bool blockEvents = false;
    
    private CursorState cursorState = CursorState.None;
    private enum CursorState
    {
        None = 0,
        HoldingObject = 1,
        DraggingCursor = 2,
    }

    private bool pointerOver = false;

    private readonly ClickDragHelper clickDrag = new();
    
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
         UpdateSettings();
         UpdateShortcuts();
    }

    private async void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        try
        {
            await Task.Delay(1);

            if (Application.Current == null) return;
            if (!Application.Current.TryGetResource("BackgroundSecondary", Application.Current.ActualThemeVariant, out object? resource)) return;
            if (resource is not SolidColorBrush brush) return;
        
            canvasInfo.BackgroundColor= new(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
        catch (Exception)
        {
            // classic error pink
            canvasInfo.BackgroundColor = new(0xFF, 0x00, 0xFF, 0xFF);
        }
    }
    
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        double minimum = double.Min(PanelCanvasContainer.Bounds.Width, PanelCanvasContainer.Bounds.Height);
        RenderCanvas.Width = minimum;
        RenderCanvas.Height = minimum;

        canvasInfo.Width = (float)RenderCanvas.Width;
        canvasInfo.Height = (float)RenderCanvas.Height;
        canvasInfo.Radius = canvasInfo.Width / 2;
        canvasInfo.Center = new(canvasInfo.Radius, canvasInfo.Radius);
    }

    private void RenderCanvas_OnRenderAction(SKCanvas canvas)
    {
        Renderer3D.Render
        (
            canvas: canvas, 
            canvasInfo: canvasInfo, 
            settings: SettingsSystem.RenderSettings, 
            chart: ChartSystem.Chart, 
            entry: ChartSystem.Entry, 
            time: TimeSystem.Timestamp.Time, 
            playing: TimeSystem.PlaybackState is PlaybackState.Playing or PlaybackState.Preview,
            selectedObjects: EditorSystem.SelectedObjects,
            pointerOverObject: EditorSystem.PointerOverObject,
            boxSelect: new(EditorSystem.BoxSelectData.GlobalStartTime, EditorSystem.BoxSelectData.GlobalEndTime, EditorSystem.BoxSelectData.Position, EditorSystem.BoxSelectData.Size)
        );
    }

    private void RenderCanvas_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(sender as Control);

        float radius = Renderer3D.GetHitTestPointerRadius(canvasInfo, (float)point.Position.X, (float)point.Position.Y);
        int lane = Renderer3D.GetHitTestPointerLane(canvasInfo, (float)point.Position.X, (float)point.Position.Y);
        float viewDistance = Renderer3D.GetViewDistance(SettingsSystem.RenderSettings.NoteSpeed);

        pointerOver = radius <= 1.1;
        
        onMove();
        
        if (e.Properties.IsLeftButtonPressed) 
        {
            onLeftDrag();
        }
        else if (e.Properties.IsRightButtonPressed) 
        {
            onRightDrag();
        }

        return;

        void onMove()
        {
            pointerOver();
            setCursor();

            return;

            void pointerOver()
            {
                if (radius > 1.1f)
                {
                    EditorSystem.PointerOverObject = null;
                    return;
                }
            
                float threshold = Renderer3D.GetHitTestThreshold(canvasInfo, SettingsSystem.RenderSettings.NoteThickness);
                
                foreach (Layer layer in ChartSystem.Chart.Layers)
                {
                    float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);

                    foreach (Event @event in layer.Events)
                    {
                        if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                        
                        IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(@event, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                        if (hitTestResult != IPositionable.OverlapResult.None)
                        {
                            EditorSystem.PointerOverObject = @event;
                            EditorSystem.PointerOverOverlap = hitTestResult;
                            return;
                        }
                    }
                    
                    foreach (Note note in layer.Notes)
                    {
                        if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                        
                        IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(note, radius, lane, TimeSystem.Timestamp.Time, scaledTime, viewDistance, threshold, SettingsSystem.RenderSettings.ShowSpeedChanges, SettingsSystem.RenderSettings);
                        if (hitTestResult != IPositionable.OverlapResult.None)
                        {
                            EditorSystem.PointerOverObject = note;
                            EditorSystem.PointerOverOverlap = hitTestResult;
                            return;
                        }
                    }
                }
                
                foreach (Note note in ChartSystem.Chart.LaneToggles)
                {
                    if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(note, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                    if (hitTestResult != IPositionable.OverlapResult.None)
                    {
                        EditorSystem.PointerOverObject = note;
                        EditorSystem.PointerOverOverlap = hitTestResult;
                        return;
                    }
                }
                
                foreach (Event @event in ChartSystem.Chart.Events)
                {
                    if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;

                    IPositionable.OverlapResult hitTestResult = Renderer3D.HitTest(@event, radius, lane, TimeSystem.Timestamp.Time, TimeSystem.Timestamp.Time, viewDistance, threshold, false, SettingsSystem.RenderSettings);
                    if (hitTestResult != IPositionable.OverlapResult.None)
                    {
                        EditorSystem.PointerOverObject = @event;
                        EditorSystem.PointerOverOverlap = hitTestResult;
                        return;
                    }
                }
                
                EditorSystem.PointerOverObject = null;
                EditorSystem.PointerOverOverlap = IPositionable.OverlapResult.None;
            }

            void setCursor()
            {
                if (EditorSystem.PointerOverOverlap == IPositionable.OverlapResult.None)
                {
                    RenderCanvas.Cursor = new(StandardCursorType.Arrow);
                }
                else if (EditorSystem.PointerOverOverlap == IPositionable.OverlapResult.Body)
                {
                    RenderCanvas.Cursor = new(StandardCursorType.SizeAll);
                }
                else
                {
                    bool rightEdge = EditorSystem.PointerOverOverlap == IPositionable.OverlapResult.RightEdge;
                    
                    if (lane is >= 0 and <= 3
                        or >= 56 and <= 59)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.TopSide) : new(StandardCursorType.BottomSide);
                    }

                    if (lane is >= 4 and <= 10)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.BottomRightCorner) : new(StandardCursorType.TopLeftCorner);
                    }

                    if (lane is >= 11 and <= 18)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.LeftSide) : new(StandardCursorType.RightSide);
                    }
                    
                    if (lane is >= 19 and <= 25)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.BottomLeftCorner) : new(StandardCursorType.TopRightCorner);
                    }
                    
                    if (lane is >= 26 and <= 33)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.BottomSide) : new(StandardCursorType.TopSide);
                    }
                    
                    if (lane is >= 34 and <= 40)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.TopLeftCorner) : new(StandardCursorType.BottomRightCorner);
                    }
                    
                    if (lane is >= 41 and <= 48)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.RightSide) : new(StandardCursorType.LeftSide);
                    }
                    
                    if (lane is >= 49 and <= 55)
                    {
                        RenderCanvas.Cursor = rightEdge ? new(StandardCursorType.TopLeftCorner) : new(StandardCursorType.BottomLeftCorner);
                    }
                }
            }
        }
        
        void onLeftDrag()
        {
            if (e.Properties.IsLeftButtonPressed == false) return;

            if (cursorState == CursorState.None)
            {
                float t = RenderUtils.InversePerspective(radius);
                float viewTime = RenderUtils.Lerp(viewDistance, 0, t);
                
                if (EditorSystem.BoxSelectData.GlobalStartTime == null)
                {
                    // Box select has just started.
                    EditorSystem.BoxSelectData.NegativeSelection = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
                    EditorSystem.BoxSelectData.GlobalStartTime = TimeSystem.Timestamp.Time + viewTime;
                    clickDrag.Reset(lane);
                    
                    EditorSystem.BoxSelectData.ScaledStartTimes.Clear();
                    foreach (Layer layer in ChartSystem.Chart.Layers)
                    {
                        float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);
                        EditorSystem.BoxSelectData.ScaledStartTimes.Add(layer, scaledTime + viewTime);    
                    }
                }
                else
                {
                    // Box select already running.
                    clickDrag.EndLane = lane;
                    
                    EditorSystem.BoxSelectData.GlobalEndTime = TimeSystem.Timestamp.Time + viewTime;
                    EditorSystem.BoxSelectData.ScaledEndTimes.Clear();
                    foreach (Layer layer in ChartSystem.Chart.Layers)
                    {
                        float scaledTime = Timestamp.ScaledTimeFromTime(layer, TimeSystem.Timestamp.Time);
                        EditorSystem.BoxSelectData.ScaledEndTimes.Add(layer, scaledTime + viewTime);    
                    }
                }

                EditorSystem.BoxSelectData.Position = clickDrag.Position;
                EditorSystem.BoxSelectData.Size = clickDrag.Size;
                
                return;
            }

            if (cursorState == CursorState.HoldingObject)
            {
                Console.WriteLine("Dragging object");
            }
        }

        void onRightDrag()
        {
            if (e.Properties.IsRightButtonPressed == false) return;

            if (cursorState == CursorState.None)
            {
                clickDrag.Reset(lane);
                CursorSystem.Position = clickDrag.Position;
                
                cursorState = CursorState.DraggingCursor;
            }

            if (cursorState == CursorState.DraggingCursor)
            {
                clickDrag.EndLane = lane;
                CursorSystem.Position = clickDrag.Position;
                CursorSystem.Size = clickDrag.Size;
            }
        }
    }

    private void RenderCanvas_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        onLeftClick();
        onRightClick();
        
        return;
        
        void onLeftClick()
        {
            if (!e.Properties.IsLeftButtonPressed) return;
            
            bool control = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            bool alt = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
            bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            
            if (!control && !shift && !alt)
            {
                EditorSystem.SelectedObjects.Clear();
                EditorSystem.LastSelectedObject = null;
            }
            
            if (EditorSystem.PointerOverObject != null)
            {
                cursorState = CursorState.HoldingObject;
                
                if (shift && EditorSystem.LastSelectedObject != null)
                {
                    Timestamp start = Timestamp.Min(EditorSystem.LastSelectedObject.Timestamp, EditorSystem.PointerOverObject.Timestamp);
                    Timestamp end = Timestamp.Max(EditorSystem.LastSelectedObject.Timestamp, EditorSystem.PointerOverObject.Timestamp);
                    List<ITimeable> objects = [];

                    foreach (Event @event in ChartSystem.Chart.Events)
                    {
                        if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                        if (@event.Timestamp < start) continue;
                        if (@event.Timestamp > end) continue;

                        objects.Add(@event);
                    }

                    foreach (Note note in ChartSystem.Chart.LaneToggles)
                    {
                        if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                        if (note.Timestamp < start) continue;
                        if (note.Timestamp > end) continue;

                        objects.Add(note);
                    }

                    foreach (Layer layer in ChartSystem.Chart.Layers)
                    {
                        foreach (Event @event in layer.Events)
                        {
                            if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                            if (@event.Timestamp < start) continue;
                            if (@event.Timestamp > end) continue;

                            objects.Add(@event);
                        }
                        
                        foreach (Note note in layer.Notes)
                        {
                            if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                            if (note.Timestamp < start) continue;
                            if (note.Timestamp > end) continue;

                            objects.Add(note);
                        }
                    }

                    EditorSystem.SelectedObjects.Clear();
                    EditorSystem.SelectedObjects.AddRange(objects);
                    EditorSystem.SelectedObjects.Add(EditorSystem.LastSelectedObject);
                    EditorSystem.SelectedObjects.Add(EditorSystem.PointerOverObject);
                }
                else
                {
                    if (!EditorSystem.SelectedObjects.Add(EditorSystem.PointerOverObject))
                    {
                        EditorSystem.SelectedObjects.Remove(EditorSystem.PointerOverObject);
                    }
                    
                    EditorSystem.LastSelectedObject = EditorSystem.PointerOverObject;
                }
            }
        }

        void onRightClick()
        {
            if (!e.Properties.IsRightButtonPressed) return;
        }
    }
    
    private void RenderCanvas_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        onLeftReleased();
        onRightReleased();
        
        return;
        
        void onLeftReleased()
        {
            if (e.InitialPressMouseButton != MouseButton.Left) return;

            cursorState = CursorState.None;
            
            if (EditorSystem.BoxSelectData.GlobalStartTime != null
                && EditorSystem.BoxSelectData.GlobalEndTime != null
                && EditorSystem.BoxSelectData.ScaledStartTimes.Count != 0
                && EditorSystem.BoxSelectData.ScaledEndTimes.Count != 0)
            {
                float globalMin = MathF.Min((float)EditorSystem.BoxSelectData.GlobalStartTime, (float)EditorSystem.BoxSelectData.GlobalEndTime);
                float globalMax = MathF.Max((float)EditorSystem.BoxSelectData.GlobalStartTime, (float)EditorSystem.BoxSelectData.GlobalEndTime);
                
                foreach (Event @event in ChartSystem.Chart.Events)
                {
                    if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                    if (@event.Timestamp.Time < globalMin) continue;
                    if (@event.Timestamp.Time > globalMax) continue;

                    if (EditorSystem.BoxSelectData.NegativeSelection)
                    {
                        EditorSystem.SelectedObjects.Remove(@event);
                    }
                    else
                    {
                        EditorSystem.SelectedObjects.Add(@event);
                    }
                }

                foreach (Note note in ChartSystem.Chart.LaneToggles)
                {
                    if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                    if (note.Timestamp.Time < globalMin) continue;
                    if (note.Timestamp.Time > globalMax) continue;

                    if (note is IPositionable positionable && !IPositionable.IsAnyOverlap(positionable.Position, positionable.Size, EditorSystem.BoxSelectData.Position, EditorSystem.BoxSelectData.Size)) continue;
                    
                    if (EditorSystem.BoxSelectData.NegativeSelection)
                    {
                        EditorSystem.SelectedObjects.Remove(note);
                    }
                    else
                    {
                        EditorSystem.SelectedObjects.Add(note);
                    }
                }

                foreach (Layer layer in ChartSystem.Chart.Layers)
                {
                    foreach (Event @event in layer.Events)
                    {
                        if (!RenderUtils.IsVisible(@event, SettingsSystem.RenderSettings)) continue;
                        if (@event.Timestamp.Time < globalMin) continue;
                        if (@event.Timestamp.Time > globalMax) continue;

                        if (EditorSystem.BoxSelectData.NegativeSelection)
                        {
                            EditorSystem.SelectedObjects.Remove(@event);
                        }
                        else
                        {
                            EditorSystem.SelectedObjects.Add(@event);
                        }
                    }
                    
                    foreach (Note note in layer.Notes)
                    {
                        if (!RenderUtils.IsVisible(note, SettingsSystem.RenderSettings)) continue;
                        
                        float min = MathF.Min(EditorSystem.BoxSelectData.ScaledStartTimes[layer], EditorSystem.BoxSelectData.ScaledEndTimes[layer]);
                        float max = MathF.Max(EditorSystem.BoxSelectData.ScaledStartTimes[layer], EditorSystem.BoxSelectData.ScaledEndTimes[layer]);
                        
                        if (note is HoldNote holdNote && holdNote.Points.Count > 1)
                        {
                            if (holdNote.Points[^1].Timestamp.ScaledTime < min) continue;
                            if (holdNote.Points[0].Timestamp.ScaledTime  > max) continue;

                            bool overlap = false;
                            foreach (HoldPointNote point in holdNote.Points)
                            {
                                if (point.Timestamp.ScaledTime < min) continue;
                                if (point.Timestamp.ScaledTime > max) continue;
                                if (!IPositionable.IsAnyOverlap(point.Position, point.Size, EditorSystem.BoxSelectData.Position, EditorSystem.BoxSelectData.Size)) continue;

                                overlap = true;
                                break;
                            }
                            
                            if (!overlap) continue;
                        }
                        else
                        {
                            if (note.Timestamp.ScaledTime < min) continue;
                            if (note.Timestamp.ScaledTime > max) continue;
                            
                            if (note is IPositionable positionable && !IPositionable.IsAnyOverlap(positionable.Position, positionable.Size, EditorSystem.BoxSelectData.Position, EditorSystem.BoxSelectData.Size)) continue;
                        }
                        
                        if (EditorSystem.BoxSelectData.NegativeSelection)
                        {
                            EditorSystem.SelectedObjects.Remove(note);
                        }
                        else
                        {
                            EditorSystem.SelectedObjects.Add(note);
                        }
                    }
                }
            }

            EditorSystem.BoxSelectData = new();
        }

        void onRightReleased()
        {
            if (e.InitialPressMouseButton != MouseButton.Right) return;

            cursorState = CursorState.None;
        }
    }
    
    private void RenderCanvas_OnPointerExited(object? sender, PointerEventArgs e)
    {
        EditorSystem.PointerOverObject = null;
    }
    
    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not MenuItem menuItem) return;
        
        if (menuItem == MenuItemShowSpeedChanges)
        {
            SettingsSystem.RenderSettings.ShowSpeedChanges = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowVisibilityChanges)
        {
            SettingsSystem.RenderSettings.ShowVisibilityChanges = menuItem.IsChecked;
            return;
        }

        if (menuItem == MenuItemShowLaneToggleAnimations)
        {
            SettingsSystem.RenderSettings.ShowLaneToggleAnimations = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowJudgeAreas)
        {
            SettingsSystem.RenderSettings.ShowJudgeAreas = menuItem.IsChecked;

            MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowMarvelousWindows)
        {
            SettingsSystem.RenderSettings.ShowMarvelousWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowGreatWindows)
        {
            SettingsSystem.RenderSettings.ShowGreatWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowGoodWindows)
        {
            SettingsSystem.RenderSettings.ShowGoodWindows = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemSaturnJudgeAreas)
        {
            SettingsSystem.RenderSettings.SaturnJudgeAreas = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemVisualizeLaneSweeps)
        {
            SettingsSystem.RenderSettings.VisualizeLaneSweeps = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTouchNotes)
        {
            SettingsSystem.RenderSettings.ShowTouchNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowChainNotes)
        {
            SettingsSystem.RenderSettings.ShowChainNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowHoldNotes)
        {
            SettingsSystem.RenderSettings.ShowHoldNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSlideClockwiseNotes)
        {
            SettingsSystem.RenderSettings.ShowSlideClockwiseNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSlideCounterclockwiseNotes)
        {
            SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSnapForwardNotes)
        {
            SettingsSystem.RenderSettings.ShowSnapForwardNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSnapBackwardNotes)
        {
            SettingsSystem.RenderSettings.ShowSnapBackwardNotes = menuItem.IsChecked;
            return;
        }

        if (menuItem == MenuItemShowSyncNotes)
        {
            SettingsSystem.RenderSettings.ShowSyncNotes = menuItem.IsChecked;
            return;
        }

        if (menuItem == MenuItemShowMeasureLineNotes)
        {
            SettingsSystem.RenderSettings.ShowMeasureLineNotes = menuItem.IsChecked;
        }
        
        if (menuItem == MenuItemShowBeatLineNotes)
        {
            SettingsSystem.RenderSettings.ShowBeatLineNotes = menuItem.IsChecked;
        }
        
        if (menuItem == MenuItemShowLaneShowNotes)
        {
            SettingsSystem.RenderSettings.ShowLaneShowNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowLaneHideNotes)
        {
            SettingsSystem.RenderSettings.ShowLaneHideNotes = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTempoChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowTempoChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowMetreChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowMetreChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowSpeedChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowSpeedChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowVisibilityChangeEvents)
        {
            SettingsSystem.RenderSettings.ShowVisibilityChangeEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowReverseEffectEvents)
        {
            SettingsSystem.RenderSettings.ShowReverseEffectEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowStopEffectEvents)
        {
            SettingsSystem.RenderSettings.ShowStopEffectEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemShowTutorialMarkerEvents)
        {
            SettingsSystem.RenderSettings.ShowTutorialMarkerEvents = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemHideEventMarkers)
        {
            SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemHideLaneToggleNotes)
        {
            SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback = menuItem.IsChecked;
            return;
        }
        
        if (menuItem == MenuItemHideHoldControlPoints)
        {
            SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback = menuItem.IsChecked;
        }
    }

    private void NumericUpDownNoteSpeed_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender == null) return;
        SettingsSystem.RenderSettings.NoteSpeed = (int)Math.Round((e.NewValue * 10) ?? 3);
    }

    private void ComboBoxBackgroundDim_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (blockEvents) return;
        if (sender is not ComboBox comboBox) return;
        SettingsSystem.RenderSettings.BackgroundDim = (RenderSettings.BackgroundDimOption)comboBox.SelectedIndex;
    }
    
    private void UpdateSettings()
    {
        blockEvents = true;

        MenuItemShowSpeedChanges.IsChecked = SettingsSystem.RenderSettings.ShowSpeedChanges;
        MenuItemShowVisibilityChanges.IsChecked = SettingsSystem.RenderSettings.ShowVisibilityChanges;
        MenuItemShowLaneToggleAnimations.IsChecked = SettingsSystem.RenderSettings.ShowLaneToggleAnimations;
        MenuItemShowJudgeAreas.IsChecked = SettingsSystem.RenderSettings.ShowJudgeAreas;
        MenuItemShowMarvelousWindows.IsChecked = SettingsSystem.RenderSettings.ShowMarvelousWindows;
        MenuItemShowGreatWindows.IsChecked = SettingsSystem.RenderSettings.ShowGreatWindows;
        MenuItemShowGoodWindows.IsChecked = SettingsSystem.RenderSettings.ShowGoodWindows;
        MenuItemSaturnJudgeAreas.IsChecked = SettingsSystem.RenderSettings.SaturnJudgeAreas;
        MenuItemVisualizeLaneSweeps.IsChecked = SettingsSystem.RenderSettings.VisualizeLaneSweeps;
        MenuItemShowTouchNotes.IsChecked = SettingsSystem.RenderSettings.ShowTouchNotes;
        MenuItemShowChainNotes.IsChecked = SettingsSystem.RenderSettings.ShowChainNotes;
        MenuItemShowHoldNotes.IsChecked = SettingsSystem.RenderSettings.ShowHoldNotes;
        MenuItemShowSlideClockwiseNotes.IsChecked = SettingsSystem.RenderSettings.ShowSlideClockwiseNotes;
        MenuItemShowSlideCounterclockwiseNotes.IsChecked = SettingsSystem.RenderSettings.ShowSlideCounterclockwiseNotes;
        MenuItemShowSnapForwardNotes.IsChecked = SettingsSystem.RenderSettings.ShowSnapForwardNotes;
        MenuItemShowSnapBackwardNotes.IsChecked = SettingsSystem.RenderSettings.ShowSnapBackwardNotes;
        MenuItemShowSyncNotes.IsChecked = SettingsSystem.RenderSettings.ShowSyncNotes;
        MenuItemShowMeasureLineNotes.IsChecked = SettingsSystem.RenderSettings.ShowMeasureLineNotes;
        MenuItemShowBeatLineNotes.IsChecked = SettingsSystem.RenderSettings.ShowBeatLineNotes;
        MenuItemShowLaneShowNotes.IsChecked = SettingsSystem.RenderSettings.ShowLaneShowNotes;
        MenuItemShowLaneHideNotes.IsChecked = SettingsSystem.RenderSettings.ShowLaneHideNotes;
        MenuItemShowTempoChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowTempoChangeEvents;
        MenuItemShowMetreChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowMetreChangeEvents;
        MenuItemShowSpeedChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowSpeedChangeEvents;
        MenuItemShowVisibilityChangeEvents.IsChecked = SettingsSystem.RenderSettings.ShowVisibilityChangeEvents;
        MenuItemShowReverseEffectEvents.IsChecked = SettingsSystem.RenderSettings.ShowReverseEffectEvents;
        MenuItemShowStopEffectEvents.IsChecked = SettingsSystem.RenderSettings.ShowStopEffectEvents;
        MenuItemShowTutorialMarkerEvents.IsChecked = SettingsSystem.RenderSettings.ShowTutorialMarkerEvents;

        MenuItemHideEventMarkers.IsChecked = SettingsSystem.RenderSettings.HideEventMarkersDuringPlayback;
        MenuItemHideLaneToggleNotes.IsChecked = SettingsSystem.RenderSettings.HideLaneToggleNotesDuringPlayback;
        MenuItemHideHoldControlPoints.IsChecked = SettingsSystem.RenderSettings.HideHoldControlPointsDuringPlayback;

        NumericUpDownNoteSpeed.Value = SettingsSystem.RenderSettings.NoteSpeed / 10.0m;
        ComboBoxBackgroundDim.SelectedIndex = (int)SettingsSystem.RenderSettings.BackgroundDim;
        
        MenuItemShowMarvelousWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
        MenuItemShowGreatWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;
        MenuItemShowGoodWindows.IsEnabled = MenuItemShowJudgeAreas.IsChecked;

        blockEvents = false;
    }

    private void UpdateShortcuts()
    {
        TextBlockShortcutBoxSelect.Text = SettingsSystem.ShortcutSettings.Shortcuts["Edit.BoxSelect"].ToString();
        TextBlockShortcutEditType.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditType"].ToString();
        TextBlockShortcutEditShape.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.EditShape"].ToString();
        TextBlockShortcutDeleteSelection.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.DeleteSelection"].ToString();
        TextBlockShortcutInsertNote.Text = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Toolbar.Insert"].ToString();
        
        MenuItemMoveSelectionBeatForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveBeatForward"].ToKeyGesture();
        MenuItemMoveSelectionBeatBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveBeatBack"].ToKeyGesture();
        MenuItemMoveSelectionMeasureForward.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveMeasureForward"].ToKeyGesture();
        MenuItemMoveSelectionMeasureBack.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveMeasureBack"].ToKeyGesture();
        MenuItemMoveClockwise.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveClockwise"].ToKeyGesture();
        MenuItemMoveCounterclockwise.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveCounterclockwise"].ToKeyGesture();
        MenuItemIncreaseSize.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.IncreaseSize"].ToKeyGesture();
        MenuItemDecreaseSize.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.DecreaseSize"].ToKeyGesture();
        MenuItemMoveClockwiseIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveClockwiseIterative"].ToKeyGesture();
        MenuItemMoveCounterclockwiseIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MoveCounterclockwiseIterative"].ToKeyGesture();
        MenuItemIncreaseSizeIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.IncreaseSizeIterative"].ToKeyGesture();
        MenuItemDecreaseSizeIterative.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.DecreaseSizeIterative"].ToKeyGesture();
        MenuItemMirrorHorizontal.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorHorizontal"].ToKeyGesture();
        MenuItemMirrorVertical.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorVertical"].ToKeyGesture();
        MenuItemMirrorCustom.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorCustom"].ToKeyGesture();
        MenuItemAdjustAxis.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.AdjustAxis"].ToKeyGesture();
        MenuItemFlipDirection.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.FlipDirection"].ToKeyGesture();
        MenuItemReverseSelection.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ReverseSelection"].ToKeyGesture();
        MenuItemScaleSelection.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleSelection"].ToKeyGesture();
        MenuItemOffsetChart.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.OffsetChart"].ToKeyGesture();
        MenuItemScaleChart.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.ScaleChart"].ToKeyGesture();
        MenuItemMirrorChart.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Transform.MirrorChart"].ToKeyGesture();
        MenuItemNotesToHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.NotesToHold"].ToKeyGesture();
        MenuItemHoldToNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.HoldToNotes"].ToKeyGesture();
        MenuItemHoldToHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.HoldToHold"].ToKeyGesture();
        MenuItemSpikeHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.SpikeHold"].ToKeyGesture();
        MenuItemSplitHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.SplitHold"].ToKeyGesture();
        MenuItemMergeHold.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Convert.MergeHold"].ToKeyGesture();

        MenuItemShowSpeedChanges.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowSpeedChanges"].ToKeyGesture();
        MenuItemShowVisibilityChanges.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowVisibilityChanges"].ToKeyGesture();
        MenuItemShowLaneToggleAnimations.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowLaneToggleAnimations"].ToKeyGesture();
        MenuItemShowJudgeAreas.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowJudgeAreas"].ToKeyGesture();
        MenuItemShowMarvelousWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowMarvelousWindows"].ToKeyGesture();
        MenuItemShowGreatWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGreatWindows"].ToKeyGesture();
        MenuItemShowGoodWindows.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ShowGoodWindows"].ToKeyGesture();
        MenuItemSaturnJudgeAreas.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.SaturnJudgeAreas"].ToKeyGesture();
        MenuItemVisualizeLaneSweeps.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.VisualizeLaneSweeps"].ToKeyGesture();
        MenuItemShowTouchNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Touch"].ToKeyGesture();
        MenuItemShowChainNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapForward"].ToKeyGesture();
        MenuItemShowHoldNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SnapBackward"].ToKeyGesture();
        MenuItemShowSlideClockwiseNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideClockwise"].ToKeyGesture();
        MenuItemShowSlideCounterclockwiseNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SlideCounterclockwise"].ToKeyGesture();
        MenuItemShowSnapForwardNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Chain"].ToKeyGesture();
        MenuItemShowSnapBackwardNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Hold"].ToKeyGesture();
        MenuItemShowSyncNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.Sync"].ToKeyGesture();
        MenuItemShowMeasureLineNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MeasureLine"].ToKeyGesture();
        MenuItemShowBeatLineNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.BeatLine"].ToKeyGesture();
        MenuItemShowLaneShowNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneShow"].ToKeyGesture();
        MenuItemShowLaneHideNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.LaneHide"].ToKeyGesture();
        MenuItemShowTempoChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TempoChange"].ToKeyGesture();
        MenuItemShowMetreChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.MetreChange"].ToKeyGesture();
        MenuItemShowSpeedChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.SpeedChange"].ToKeyGesture();
        MenuItemShowVisibilityChangeEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.VisibilityChange"].ToKeyGesture();
        MenuItemShowReverseEffectEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.ReverseEffect"].ToKeyGesture();
        MenuItemShowStopEffectEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.StopEffect"].ToKeyGesture();
        MenuItemShowTutorialMarkerEvents.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.ToggleVisibility.TutorialMarker"].ToKeyGesture();

        MenuItemHideEventMarkers.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.EventMarkers"].ToKeyGesture();
        MenuItemHideLaneToggleNotes.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.LaneToggleNotes"].ToKeyGesture();
        MenuItemHideHoldControlPoints.InputGesture = SettingsSystem.ShortcutSettings.Shortcuts["Editor.Settings.HideDuringPlayback.HoldControlPoints"].ToKeyGesture();
    }
}
