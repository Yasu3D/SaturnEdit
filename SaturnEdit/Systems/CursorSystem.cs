using System;
using SaturnData.Notation.Core;
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
}