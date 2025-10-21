using System;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;

namespace SaturnEdit.Systems;

public static class CursorSystem
{
    public static void Initialize() { }
    
    public static event EventHandler? TypeChanged;
    public static event EventHandler? ShapeChanged;
    
    public static Note CurrentNote
    {
        get => currentNote;
        set
        {
            if (currentNote == value) return;
            
            if (value is IPositionable positionable)
            {
                positionable.Position = Position;
                positionable.Size = Size;
            }

            if (value is ILaneToggle laneToggle)
            {
                laneToggle.Direction = Direction;
            }

            if (value is HoldPointNote holdPoint)
            {
                holdPoint.RenderType = RenderType;
            }

            currentNote = value;
            
            TypeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static Note currentNote = new TouchNote(Timestamp.Zero, 30, 15, BonusType.Normal, JudgementType.Normal);
    
    public static int Position
    {
        get => currentNote is IPositionable positionable ? positionable.Position : backupPosition;
        set
        {
            if (backupPosition == value) return;

            if (currentNote is IPositionable positionable)
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
        get => currentNote is IPositionable positionable ? positionable.Size : backupSize;
        set
        {
            if (backupSize == value) return;

            if (currentNote is IPositionable positionable)
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
        get => currentNote is IPlayable playable ? playable.BonusType : backupBonusType;
        set
        {
            if (backupBonusType == value) return;

            if (currentNote is IPlayable playable)
            {
                playable.BonusType = value;
            }
            
            backupBonusType = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static BonusType backupBonusType;
    
    public static JudgementType JudgementType
    {
        get => currentNote is IPlayable playable ? playable.JudgementType : backupJudgementType;
        set
        {
            if (backupJudgementType == value) return;

            if (currentNote is IPlayable playable)
            {
                playable.JudgementType = value;
            }
            
            backupJudgementType = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static JudgementType backupJudgementType;
    
    public static HoldPointRenderType RenderType
    {
        get => currentNote is HoldPointNote holdPoint ? holdPoint.RenderType : backupRenderType;
        set
        {
            if (backupRenderType == value) return;

            if (currentNote is HoldPointNote holdPoint)
            {
                holdPoint.RenderType = value;
            }
            
            backupRenderType = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static HoldPointRenderType backupRenderType;
    
    public static LaneSweepDirection Direction
    {
        get => currentNote is ILaneToggle laneToggle ? laneToggle.Direction : backupDirection;
        set
        {
            if (backupDirection == value) return;

            if (currentNote is ILaneToggle laneToggle)
            {
                laneToggle.Direction = value;
            }
            
            backupDirection = value;
            ShapeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static LaneSweepDirection backupDirection;
}