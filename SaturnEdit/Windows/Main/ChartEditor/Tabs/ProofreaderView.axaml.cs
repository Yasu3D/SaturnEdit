using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnData.Notation.Core;
using SaturnData.Notation.Events;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.Controls;
using SaturnEdit.Systems;
using SaturnEdit.Utilities;
using SaturnEdit.Windows.Dialogs.ModalDialog;
using SaturnEdit.Windows.Dialogs.ProofreaderCriteria;

namespace SaturnEdit.Windows.Main.ChartEditor.Tabs;

public partial class ProofreaderView : UserControl
{
    public ProofreaderView()
    {
        InitializeComponent();
    }

    private static event EventHandler? ProofreadChanged;
    
    private static ProofreaderCriteria criteria = new();
    private static readonly List<ProofreaderProblem> Problems = [];
    
    private bool blockEvents = false;

    private IDisposable? keyDownEventHandler = null;
    private IDisposable? keyUpEventHandler = null;

#region Methods
    private void RunProofreader()
    {
        Problems.Clear();

        if (criteria.StrictNoteSizeMer)
        {
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Note note in layer.Notes)
            {
                if (note is not IPositionable positionable) continue;
                if (note is not IPlayable playable) continue;

                int minSize = (note, playable.BonusType) switch
                {
                    (TouchNote, BonusType.Normal) => 4,
                    (TouchNote, BonusType.Bonus) => 5,
                    (TouchNote, BonusType.R) => 6,
                    (SlideClockwiseNote, BonusType.Normal) => 5,
                    (SlideClockwiseNote, BonusType.Bonus) => 7,
                    (SlideClockwiseNote, BonusType.R) => 10,
                    (SlideCounterclockwiseNote, BonusType.Normal) => 5,
                    (SlideCounterclockwiseNote, BonusType.Bonus) => 7,
                    (SlideCounterclockwiseNote, BonusType.R) => 10,
                    (SnapForwardNote, BonusType.Normal) => 6,
                    (SnapForwardNote, BonusType.Bonus) => 6,
                    (SnapForwardNote, BonusType.R) => 8,
                    (SnapBackwardNote, BonusType.Normal) => 6,
                    (SnapBackwardNote, BonusType.Bonus) => 6,
                    (SnapBackwardNote, BonusType.R) => 8,
                    (ChainNote, BonusType.Normal) => 4,
                    (ChainNote, BonusType.Bonus) => 4,
                    (ChainNote, BonusType.R) => 10,
                    (HoldNote, BonusType.Normal) => 2,
                    (HoldNote, BonusType.Bonus) => 2,
                    (HoldNote, BonusType.R) => 8,
                    (_, _) => 1,
                };

                if (positionable.Size >= minSize) continue;
                
                Problems.Add(new()
                {
                    Measure = note.Timestamp.Measure,
                    Tick = note.Timestamp.Tick,
                    Position = positionable.Position,
                    Size = positionable.Size,
                    ProblemKey = "ChartEditor.Proofreader.Problem.StrictNoteSizeMer",
                    TypeKey = typeKey(note),
                });
            }
        }
        else if (criteria.StrictNoteSizeSat)
        {
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Note note in layer.Notes)
            {
                if (note is not IPositionable positionable) continue;
                if (note is not IPlayable) continue;
                
                if (positionable.Size > 2) continue;
                if (note is HoldNote && positionable.Size >= 2) continue;
                if (note is SyncNote && positionable.Size >= 1) continue;
                
                Problems.Add(new()
                {
                    Measure = note.Timestamp.Measure,
                    Tick = note.Timestamp.Tick,
                    Position = positionable.Position,
                    Size = positionable.Size,
                    ProblemKey = "ChartEditor.Proofreader.Problem.StrictNoteSizeSat",
                    TypeKey = typeKey(note),
                });
            }
        }

        if (criteria.StrictBonusTypeMer)
        {
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Note note in layer.Notes)
            {
                if (note is not IPositionable positionable) continue;
                if (note is not IPlayable playable) continue;
                if (playable.BonusType != BonusType.Bonus) continue;
                if (note is TouchNote or SlideClockwiseNote or SlideCounterclockwiseNote) continue;
                
                Problems.Add(new()
                {
                    Measure = note.Timestamp.Measure,
                    Tick = note.Timestamp.Tick,
                    Position = positionable.Position,
                    Size = positionable.Size,
                    ProblemKey = "ChartEditor.Proofreader.Problem.StrictBonusTypeMer",
                    TypeKey = typeKey(note),
                });
            }
        }

        if (criteria.OverlappingNotesLenient || criteria.OverlappingNotesStrict)
        {
            Dictionary<int, List<Note>> notesOnTick = [];
            
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Note note in layer.Notes)
            {
                if (note is not IPositionable) continue;
                if (note is not IPlayable) continue;

                if (note is HoldNote) continue;
                if (!criteria.OverlappingNotesStrict && note is ChainNote) continue;
                
                if (notesOnTick.TryGetValue(note.Timestamp.FullTick, out List<Note>? notes))
                {
                    notes.Add(note);
                    continue;
                }
                
                notesOnTick[note.Timestamp.FullTick] = [ note ];
            }

            foreach (KeyValuePair<int, List<Note>> notes in notesOnTick)
            {
                if (notes.Value.Count < 2) continue;

                Note root = notes.Value[0];
                
                for (int i = 1; i < notes.Value.Count; i++)
                {
                    Note compare = notes.Value[i];

                    if (!IPositionable.IsAnyOverlap((IPositionable)root, (IPositionable)compare)) continue;
                    
                    Problems.Add(new()
                    {
                        Measure = compare.Timestamp.Measure,
                        Tick = compare.Timestamp.Tick,
                        Position = ((IPositionable)compare).Position,
                        Size = ((IPositionable)compare).Size,
                        ProblemKey = "ChartEditor.Proofreader.Problem.OverlappingNotes",
                        TypeKey = typeKey(compare),
                    });
                }
            }
        }

        if (criteria.AmbiguousHoldNoteDefinition)
        {
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Note note in layer.Notes)
            {
                if (note is not HoldNote holdNote) continue;
                if (holdNote.Points.Count < 2) continue;

                for (int i = 1; i < holdNote.Points.Count; i++)
                {
                    HoldPointNote previousPoint = holdNote.Points[i - 1];
                    HoldPointNote currentPoint = holdNote.Points[i];

                    int previousCenter = previousPoint.Position * 2 + previousPoint.Size;
                    int currentCenter = currentPoint.Position * 2 + currentPoint.Size;

                    if (Math.Abs(previousCenter - currentCenter) != 60) continue;
                    
                    Problems.Add(new()
                    {
                        Measure = previousPoint.Timestamp.Measure,
                        Tick = previousPoint.Timestamp.Tick,
                        Position = previousPoint.Position,
                        Size = previousPoint.Size,
                        ProblemKey = "ChartEditor.Proofreader.Problem.AmbiguousHoldNoteDefinition",
                        TypeKey = typeKey(previousPoint),
                    });
                }
            }
        }
        
        if (criteria.EffectsOnLowers && ChartSystem.Entry.Difficulty is Difficulty.Normal or Difficulty.Hard)
        {
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Event @event in layer.Events)
            {
                Problems.Add(new()
                {
                    Measure = @event.Timestamp.Measure,
                    Tick = @event.Timestamp.Tick,
                    Position = -1,
                    Size = -1,
                    ProblemKey = "ChartEditor.Proofreader.Problem.OverlappingNotes",
                    TypeKey = typeKey(@event),
                });
            }
        }

        if (criteria.InvalidEffectsMer)
        {
            // Speed Changes before Stops
            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                StopEffectEvent? lastStop = null;
                
                for (int i = layer.Events.Count - 1; i >= 0; i--)
                {
                    Event @event = layer.Events[i];

                    if (@event is StopEffectEvent stop)
                    {
                        lastStop = stop;
                        continue;
                    }

                    if (@event is SpeedChangeEvent && lastStop != null)
                    {
                        Problems.Add(new()
                        {
                            Measure = @event.Timestamp.Measure,
                            Tick = @event.Timestamp.Tick,
                            Position = -1,
                            Size = -1,
                            ProblemKey = "ChartEditor.Proofreader.Problem.SpeedChangeBeforeStopEffect",
                            TypeKey = typeKey(@event),
                        });
                    }
                }
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Event @event in layer.Events)
            {
                Problems.Add(new()
                {
                    Measure = @event.Timestamp.Measure,
                    Tick = @event.Timestamp.Tick,
                    Position = -1,
                    Size = -1,
                    ProblemKey = "ChartEditor.Proofreader.Problem.SpeedChangeBeforeStopEffect",
                    TypeKey = typeKey(@event),
                });
            }

            // Speed Changes too close together.
            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                List<SpeedChangeEvent> speedChanges = layer.Events.OfType<SpeedChangeEvent>().ToList();

                for (int i = 1; i < speedChanges.Count; i++)
                {
                    SpeedChangeEvent current = speedChanges[i];
                    SpeedChangeEvent previous = speedChanges[i - 1];

                    int delta = Math.Abs(current.Timestamp.FullTick - previous.Timestamp.FullTick);
                    if (delta == 0) continue;
                    if (delta >= 120) continue;
                    
                    Problems.Add(new()
                    {
                        Measure = previous.Timestamp.Measure,
                        Tick = previous.Timestamp.Tick,
                        Position = -1,
                        Size = -1,
                        ProblemKey = "ChartEditor.Proofreader.Problem.SpeedChangeTooClose",
                        TypeKey = typeKey(previous),
                    });
                }
            }
            
            // Speed Change before FullTick = 240
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Event @event in layer.Events)
            {
                if (@event is not SpeedChangeEvent) continue;
                if (@event.Timestamp.FullTick >= 240) continue;
                
                Problems.Add(new()
                {
                    Measure = @event.Timestamp.Measure,
                    Tick = @event.Timestamp.Tick,
                    Position = -1,
                    Size = -1,
                    ProblemKey = "ChartEditor.Proofreader.Problem.SpeedChangeTooEarly",
                    TypeKey = typeKey(@event),
                });
            }
            
            // Speed with near-zero value.
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Event @event in layer.Events)
            {
                if (@event is not SpeedChangeEvent speedChangeEvent) continue;
                if (speedChangeEvent.Speed == 0) continue;
                if (Math.Abs(speedChangeEvent.Speed) >= 0.1) continue;
                
                Problems.Add(new()
                {
                    Measure = @event.Timestamp.Measure,
                    Tick = @event.Timestamp.Tick,
                    Position = -1,
                    Size = -1,
                    ProblemKey = "ChartEditor.Proofreader.Problem.SpeedChangeNearZero",
                    TypeKey = typeKey(@event),
                });
            }
            
            // Multiple Reverses
            bool hasReverse = false;
            foreach (Layer layer in ChartSystem.Chart.Layers)
            foreach (Event @event in layer.Events)
            {
                if (@event is not ReverseEffectEvent) continue;

                if (hasReverse)
                {
                    Problems.Add(new()
                    {
                        Measure = @event.Timestamp.Measure,
                        Tick = @event.Timestamp.Tick,
                        Position = -1,
                        Size = -1,
                        ProblemKey = "ChartEditor.Proofreader.Problem.MultipleReverses",
                        TypeKey = typeKey(@event),
                    });
                        
                    continue;
                }

                hasReverse = true;
            }
        }

        if (criteria.InvalidLaneToggles)
        {
            for (int i = 0; i < ChartSystem.Chart.LaneToggles.Count; i++)
            {
                Note current = ChartSystem.Chart.LaneToggles[i];
                if (current is not IPositionable currentPositionable) continue;
                
                float sweepEnd = current.Timestamp.Time + ILaneToggle.SweepDuration(((ILaneToggle)currentPositionable).Direction, currentPositionable.Size);
                
                for (int j = i + 1; j < ChartSystem.Chart.LaneToggles.Count; j++)
                {
                    Note next = ChartSystem.Chart.LaneToggles[j];

                    if (next is not IPositionable nextPositionable) continue;
                    if (!IPositionable.IsAnyOverlap(currentPositionable, nextPositionable)) continue;
                    
                    if (next.Timestamp.Time < current.Timestamp.Time) continue;
                    if (next.Timestamp.Time >= sweepEnd) break;
                    
                    Problems.Add(new()
                    {
                        Measure = next.Timestamp.Measure,
                        Tick = next.Timestamp.Tick,
                        Position = -1,
                        Size = -1,
                        ProblemKey = "ChartEditor.Proofreader.Problem.InvalidLaneToggles",
                        TypeKey = typeKey(next),
                    });
                }
            }
        }

        if (criteria.NotesDuringReverse)
        {
            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                List<ReverseEffectEvent> reverses = layer.Events.OfType<ReverseEffectEvent>().ToList();

                foreach (Note note in layer.Notes)
                {
                    int position = -1;
                    int size = -1;

                    if (note is IPositionable positionable)
                    {
                        position = positionable.Position;
                        size = positionable.Size;
                    }
                    
                    foreach (ReverseEffectEvent reverse in reverses)
                    {
                        if (note.Timestamp.FullTick < reverse.SubEvents[0].Timestamp.FullTick) continue;
                        if (note.Timestamp.FullTick > reverse.SubEvents[1].Timestamp.FullTick) continue;
                        
                        Problems.Add(new()
                        {
                            Measure = note.Timestamp.Measure,
                            Tick = note.Timestamp.Tick,
                            Position = position,
                            Size = size,
                            ProblemKey = "ChartEditor.Proofreader.Problem.NotesDuringReverse",
                            TypeKey = typeKey(note),
                        });
                        break;
                    }
                }
            }
        }
        
        if (criteria.ObjectsAfterChartEnd)
        {
            foreach (Event @event in ChartSystem.Chart.Events)
            {
                if (@event.Timestamp.FullTick < ChartSystem.Entry.ChartEnd.FullTick) continue;
                
                Problems.Add(new()
                {
                    Measure = @event.Timestamp.Measure,
                    Tick = @event.Timestamp.Tick,
                    Position = -1,
                    Size = -1,
                    ProblemKey = "ChartEditor.Proofreader.Problem.ObjectsAfterChartEnd",
                    TypeKey = typeKey(@event),
                });
            }

            foreach (Note laneToggle in ChartSystem.Chart.LaneToggles)
            {
                if (laneToggle.Timestamp.FullTick < ChartSystem.Entry.ChartEnd.FullTick) continue;
                
                Problems.Add(new()
                {
                    Measure = laneToggle.Timestamp.Measure,
                    Tick = laneToggle.Timestamp.Tick,
                    Position = ((IPositionable)laneToggle).Position,
                    Size = ((IPositionable)laneToggle).Size,
                    ProblemKey = "ChartEditor.Proofreader.Problem.ObjectsAfterChartEnd",
                    TypeKey = typeKey(laneToggle),
                });
            }

            foreach (Layer layer in ChartSystem.Chart.Layers)
            {
                foreach (Event @event in layer.Events)
                {
                    if (@event.Timestamp.FullTick < ChartSystem.Entry.ChartEnd.FullTick) continue;
                
                    Problems.Add(new()
                    {
                        Measure = @event.Timestamp.Measure,
                        Tick = @event.Timestamp.Tick,
                        Position = -1,
                        Size = -1,
                        ProblemKey = "ChartEditor.Proofreader.Problem.ObjectsAfterChartEnd",
                        TypeKey = typeKey(@event),
                    });
                }

                foreach (Note note in layer.Notes)
                {
                    if (note.Timestamp.FullTick < ChartSystem.Entry.ChartEnd.FullTick) continue;
                    if (note is IPlayable playable && playable.JudgementType == JudgementType.Fake) continue;
                    
                    int position = -1;
                    int size = -1;

                    if (note is IPositionable positionable)
                    {
                        position = positionable.Position;
                        size = positionable.Size;
                    }
                    
                    Problems.Add(new()
                    {
                        Measure = note.Timestamp.Measure,
                        Tick = note.Timestamp.Tick,
                        Position = position,
                        Size = size,
                        ProblemKey = "ChartEditor.Proofreader.Problem.ObjectsAfterChartEnd",
                        TypeKey = typeKey(note),
                    });
                }
            }
        }
        
        ProofreadChanged?.Invoke(null, EventArgs.Empty);
        return;

        string typeKey(ITimeable obj)
        {
            return obj switch
            {
                TouchNote                 => "ChartEditor.General.Type.Value.Touch",
                SnapForwardNote           => "ChartEditor.General.Type.Value.SnapForward",
                SnapBackwardNote          => "ChartEditor.General.Type.Value.SnapBackward",
                SlideClockwiseNote        => "ChartEditor.General.Type.Value.SlideClockwise",
                SlideCounterclockwiseNote => "ChartEditor.General.Type.Value.SlideCounterclockwise",
                ChainNote                 => "ChartEditor.General.Type.Value.Chain",
                HoldNote                  => "ChartEditor.General.Type.Value.Hold",
                HoldPointNote             => "ChartEditor.General.Type.Value.HoldPoint",
                LaneShowNote              => "ChartEditor.General.Type.Value.LaneShow",
                LaneHideNote              => "ChartEditor.General.Type.Value.LaneHide",
                MeasureLineNote           => "ChartEditor.General.Type.Value.MeasureLine",
                SyncNote                  => "ChartEditor.General.Type.Value.Sync",
                TempoChangeEvent          => "ChartEditor.General.Type.Value.TempoChange",
                MetreChangeEvent          => "ChartEditor.General.Type.Value.MetreChange",
                TutorialMarkerEvent       => "ChartEditor.General.Type.Value.TutorialMarker",
                SpeedChangeEvent          => "ChartEditor.General.Type.Value.SpeedChange",
                VisibilityChangeEvent     => "ChartEditor.General.Type.Value.VisibilityChange",
                StopEffectEvent           => "ChartEditor.General.Type.Value.StopEffect",
                ReverseEffectEvent        => "ChartEditor.General.Type.Value.ReverseEffect",
                Bookmark                  => "ChartEditor.General.Type.Value.Bookmark",
                _ => "",
            };
        }
    }
    
    private void UpdateProblems()
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            for (int i = 0; i < Problems.Count; i++)
            {
                ProofreaderProblem problem = Problems[i];
                
                if (i < ListBoxProblems.Items.Count)
                {
                    // Modify existing item.
                    if (ListBoxProblems.Items[i] is not ProofreaderProblemItem item) continue;

                    item.SetProblem(problem);
                }
                else
                {
                    // Create new item.
                    ProofreaderProblemItem item = new();
                    item.SetProblem(problem);
                    
                    ListBoxProblems.Items.Add(item);
                }
            }
            
            // Delete redundant items.
            for (int i = ListBoxProblems.Items.Count - 1; i >= Problems.Count; i--)
            {
                if (ListBoxProblems.Items[i] is not ProofreaderProblemItem item) continue;
                
                ListBoxProblems.Items.Remove(item);
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
            TextBlockShortcutRunProofreader.Text = SettingsSystem.ShortcutSettings.Shortcuts["Proofreader.Run"].ToString();
        });
    }
    
    private void OnChartLoaded(object? sender, EventArgs e)
    {
        Problems.Clear();
        UpdateProblems();
    }
    
    private void OnProofreadChanged(object? sender, EventArgs e)
    {
        UpdateProblems();
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        keyDownEventHandler = KeyDownEvent.AddClassHandler<TopLevel>(Control_OnKeyDown, RoutingStrategies.Tunnel);
        keyUpEventHandler = KeyUpEvent.AddClassHandler<TopLevel>(Control_OnKeyUp, RoutingStrategies.Tunnel);
        
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);

        ChartSystem.ChartLoaded += OnChartLoaded;
        
        ProofreadChanged += OnProofreadChanged;
        OnProofreadChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        ChartSystem.ChartLoaded -= OnChartLoaded;
        ProofreadChanged -= OnProofreadChanged;
        keyDownEventHandler?.Dispose();
        keyUpEventHandler?.Dispose();
        
        base.OnUnloaded(e);
    }
    
    private void Control_OnKeyDown(object? sender, KeyEventArgs e)
    {
        IInputElement? focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (KeyDownBlacklist.IsInvalidFocusedElement(focusedElement)) return;
        if (KeyDownBlacklist.IsInvalidKey(e.Key)) return;
        if (KeyDownBlacklist.IsInvalidState()) return;
        
        Shortcut shortcut = new(e.Key, e.KeyModifiers.HasFlag(KeyModifiers.Control), e.KeyModifiers.HasFlag(KeyModifiers.Alt), e.KeyModifiers.HasFlag(KeyModifiers.Shift));
        
        if (shortcut.Equals(SettingsSystem.ShortcutSettings.Shortcuts["Proofreader.Run"]))
        {
            Task.Run(RunProofreader);
            e.Handled = true;
        }
    }
    
    private void Control_OnKeyUp(object? sender, KeyEventArgs e) => e.Handled = true;
    
    private async void ButtonProofreadCriteria_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (VisualRoot is not Window window) return;

            ProofreaderCriteriaWindow proofreaderCriteriaWindow = new();
            proofreaderCriteriaWindow.Position = MainWindow.DialogPopupPosition(proofreaderCriteriaWindow.Width, proofreaderCriteriaWindow.Height);

            proofreaderCriteriaWindow.SetCriteria(criteria);
            
            await proofreaderCriteriaWindow.ShowDialog(window);

            if (proofreaderCriteriaWindow.Result == ModalDialogResult.Primary)
            {
                criteria = proofreaderCriteriaWindow.Criteria;
            }
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }

    private void ButtonRunProofread_OnClick(object? sender, RoutedEventArgs e) => Task.Run(RunProofreader);
    
    private void ListBoxProblems_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (blockEvents) return;
        if (ListBoxProblems.SelectedItem is not ProofreaderProblemItem item) return;
        if (item.Problem == null) return;

        TimeSystem.SeekMeasureTick(item.Problem.Value.Measure, item.Problem.Value.Tick);
    }
#endregion UI Event Handlers
}

public struct ProofreaderCriteria()
{
    public bool StrictNoteSizeMer = false;
    public bool StrictNoteSizeSat = true;
    public bool StrictBonusTypeMer = false;
    public bool OverlappingNotesLenient = true;
    public bool OverlappingNotesStrict = false;
    public bool AmbiguousHoldNoteDefinition = true;
    public bool EffectsOnLowers = true;
    public bool InvalidEffectsMer = false;
    public bool InvalidLaneToggles = true;
    public bool NotesDuringReverse = true;
    public bool ObjectsAfterChartEnd = true;
}

public struct ProofreaderProblem()
{
    public int Measure = 0;
    public int Tick = 0;
    public string TypeKey = "";
    public int Position = 0;
    public int Size;
    public string ProblemKey = "";
}