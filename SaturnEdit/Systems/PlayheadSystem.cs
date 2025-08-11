using System;
using SaturnData.Notation.Core;

namespace SaturnEdit.Systems;

public static class PlayheadSystem
{
    public static event EventHandler? TimestampChanged;
    public static event EventHandler? DivisionChanged;
    
    private static Timestamp timestamp;
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

    private static int division;
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
}