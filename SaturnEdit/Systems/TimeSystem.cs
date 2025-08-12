using System;
using SaturnData.Notation.Core;

namespace SaturnEdit.Systems;

public static class TimeSystem
{
    public static event EventHandler? TimestampChanged;
    public static event EventHandler? DivisionChanged;
    
    public const int DefaultDivision = 8;
    
    /// <summary>
    /// The current timestamp of the "playhead"
    /// </summary>
    public static Timestamp Timestamp
    {
        get => timestamp;
        set
        {
            if (timestamp != value)
            {
                timestamp = value;
                TimestampChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static Timestamp timestamp;
    
    /// <summary>
    /// The current beat division.
    /// </summary>
    public static int Division
    {
        get => division;
        set
        {
            if (division != value)
            {
                division = value;
                DivisionChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    private static int division = DefaultDivision;

    /// <summary>
    /// The number of ticks between each beat.
    /// </summary>
    public static int DivisionInterval => 1920 / Math.Max(1, Division);
}