using System;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;

namespace SaturnEdit.Systems;

public static class CursorSystem
{
    public static event EventHandler? ShapeChanged;
    public static event EventHandler? TypeChanged;
    
    /// <summary>
    /// The position of the note cursor.<br/>
    /// Follows standard note shape definitions.
    /// </summary>
    public static int Position
    {
        get => position;
        set
        {
            if (position != value)
            {
                position = value;
                ShapeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static int position = 30;
    
    /// <summary>
    /// The size of the note cursor.<br/>
    /// Follows standard note shape definitions.
    /// </summary>
    public static int Size
    {
        get => size;
        set
        {
            if (size != value)
            {
                size = value;
                ShapeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static int size = 15;

    public static Type CurrentNoteType
    {
        get => currentNoteType;
        set
        {
            if (value.BaseType != typeof(Note)) return;
            
            if (currentNoteType != value)
            {
                currentNoteType = value;
                TypeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static Type currentNoteType = typeof(TouchNote);

    public static BonusType CurrentBonusType
    {
        get => currentBonusType;
        set
        {
            if (currentBonusType != value)
            {
                currentBonusType = value;
                TypeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static BonusType currentBonusType = BonusType.Normal;
    
    public static HoldPointRenderType CurrentHoldPointRenderType
    {
        get => currentHoldPointRenderType;
        set
        {
            if (currentHoldPointRenderType != value)
            {
                currentHoldPointRenderType = value;
                TypeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static HoldPointRenderType currentHoldPointRenderType = HoldPointRenderType.Visible;
    
    public static JudgementType CurrentJudgementType
    {
        get => currentJudgementType;
        set
        {
            if (currentJudgementType != value)
            {
                currentJudgementType = value;
                TypeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static JudgementType currentJudgementType = JudgementType.Normal;
    
    public static LaneSweepDirection CurrentSweepDirection
    {
        get => currentSweepDirection;
        set
        {
            if (currentSweepDirection != value)
            {
                currentSweepDirection = value;
                TypeChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static LaneSweepDirection currentSweepDirection = LaneSweepDirection.Center;
}