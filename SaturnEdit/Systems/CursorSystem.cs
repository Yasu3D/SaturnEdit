using System;
using System.Collections.Generic;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;
using SaturnEdit.UndoRedo;
using SaturnEdit.UndoRedo.EditModeOperations;

namespace SaturnEdit.Systems;

public static class CursorSystem
{
    public static void Initialize()
    {
        TouchNote = new(Timestamp.Zero, Position, Size, BonusType, JudgementType);
        ChainNote = new(Timestamp.Zero, Position, Size, BonusType, JudgementType);
        
        HoldNote      = new(BonusType, JudgementType);
        HoldPointNote = new(Timestamp.Zero, Position, Size, HoldNote, RenderType);
        HoldNote.Points.Add(HoldPointNote);
        
        SlideClockwiseNote        = new(Timestamp.Zero, Position, Size, BonusType, JudgementType);
        SlideCounterclockwiseNote = new(Timestamp.Zero, Position, Size, BonusType, JudgementType);
        
        SnapForwardNote  = new(Timestamp.Zero, Position, Size, BonusType, JudgementType);
        SnapBackwardNote = new(Timestamp.Zero, Position, Size, BonusType, JudgementType);
        
        LaneShowNote = new(Timestamp.Zero, Position, Size, Direction);
        LaneHideNote = new(Timestamp.Zero, Position, Size, Direction);
        
        SyncNote        = new(Timestamp.Zero, Position, Size);
        MeasureLineNote = new(Timestamp.Zero, false);
    }
    
    public static event EventHandler? ShapeChanged;
    
    public static Note CurrentType
    {
        get => currentType;
        set
        {
            if (currentType == value) return;
            
            if (value is IPositionable positionable)
            {
                positionable.Position = Position;
                positionable.Size = Size;
            }

            if (value is IPlayable playable)
            {
                playable.BonusType = BonusType;
                playable.JudgementType = JudgementType;
            }

            if (value is ILaneToggle laneToggle)
            {
                laneToggle.Direction = Direction;
            }

            if (value is HoldPointNote holdPoint)
            {
                holdPoint.RenderType = RenderType;
            }

            currentType = value;
        }
    }
    private static Note currentType = new TouchNote(Timestamp.Zero, 30, 15, BonusType.Normal, JudgementType.Normal);
    
    public static int Position
    {
        get => currentType is IPositionable positionable ? positionable.Position : backupPosition;
        set
        {
            if (backupPosition == value) return;

            value = IPositionable.LimitPosition(value);
            
            if (currentType is IPositionable positionable)
            {
                positionable.Position = value;
            }
            
            backupPosition = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static int backupPosition = 30;

    public static int Size
    {
        get => currentType is IPositionable positionable ? positionable.Size : backupSize;
        set
        {
            if (backupSize == value) return;

            value = IPositionable.LimitSize(value);
            
            if (currentType is IPositionable positionable)
            {
                positionable.Size = value;
            }
            
            backupSize = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static int backupSize = 15;

    public static BonusType BonusType
    {
        get => currentType is IPlayable playable ? playable.BonusType : backupBonusType;
        set
        {
            if (backupBonusType == value) return;

            if (currentType is IPlayable playable)
            {
                playable.BonusType = value;
            }
            
            backupBonusType = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static BonusType backupBonusType = BonusType.Normal;
    
    public static JudgementType JudgementType
    {
        get => currentType is IPlayable playable ? playable.JudgementType : backupJudgementType;
        set
        {
            if (backupJudgementType == value) return;

            if (currentType is IPlayable playable)
            {
                playable.JudgementType = value;
            }
            
            backupJudgementType = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static JudgementType backupJudgementType = JudgementType.Normal;
    
    public static HoldPointRenderType RenderType
    {
        get => currentType is HoldPointNote holdPoint ? holdPoint.RenderType : backupRenderType;
        set
        {
            if (backupRenderType == value) return;

            if (currentType is HoldPointNote holdPoint)
            {
                holdPoint.RenderType = value;
            }
            
            backupRenderType = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static HoldPointRenderType backupRenderType = HoldPointRenderType.Visible;
    
    public static LaneSweepDirection Direction
    {
        get => currentType is ILaneToggle laneToggle ? laneToggle.Direction : backupDirection;
        set
        {
            if (backupDirection == value) return;

            if (currentType is ILaneToggle laneToggle)
            {
                laneToggle.Direction = value;
            }
            
            backupDirection = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static LaneSweepDirection backupDirection = LaneSweepDirection.Center;

    public static TouchNote TouchNote { get; private set; } = null!;
    public static ChainNote ChainNote { get; private set; } = null!;
    public static HoldNote HoldNote { get; private set; } = null!;
    public static HoldPointNote HoldPointNote { get; private set; } = null!;
    public static SlideClockwiseNote SlideClockwiseNote { get; private set; } = null!;
    public static SlideCounterclockwiseNote SlideCounterclockwiseNote { get; private set; } = null!;
    public static SnapForwardNote SnapForwardNote { get; private set; } = null!;
    public static SnapBackwardNote SnapBackwardNote { get; private set; } = null!;
    public static LaneShowNote LaneShowNote { get; private set; } = null!;
    public static LaneHideNote LaneHideNote { get; private set; } = null!;
    public static SyncNote SyncNote { get; private set; } = null!;
    public static MeasureLineNote MeasureLineNote { get; private set; } = null!;

#region Methods
    public static void SetType(Note newType)
    {
        List<IOperation> operations = [new CursorTypeChangeOperation(CurrentType, newType)];

        // Exit edit mode when changing to another type.
        if (newType != HoldPointNote && EditorSystem.Mode == EditorMode.EditMode && EditorSystem.ActiveObjectGroup is HoldNote)
        {
            CompositeOperation? op = EditorSystem.GetEditModeChangeOperation(EditorMode.ObjectMode, null, newType);

            if (op != null)
            {
                operations.Add(op);
            }
        }
        
        UndoRedoSystem.Push(new CompositeOperation(operations));
    }
#endregion Methods
}