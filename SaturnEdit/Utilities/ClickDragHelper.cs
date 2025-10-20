using System;

namespace SaturnEdit.Utilities;

public class ClickDragHelper
{ 
    public int StartLane { get; set; } = 0;

    public int EndLane 
    {
        get => endLane; 
        set
        {
            if (endLane == value) return;

            if (Math.Abs(endLane - value) > 30)
            {
                PassedThroughSeam = !PassedThroughSeam;
            }

            endLane = value;
        }
    }
    private int endLane = 0;
    
    public bool PassedThroughSeam { get; private set; }= false;

    public int Position => PassedThroughSeam
        ? Math.Max(StartLane, EndLane)
        : Math.Min(StartLane, EndLane);

    public int Size
    {
        get
        {
            int size = StartLane < EndLane
                ? (60 + EndLane - StartLane) % 60 + 1
                : (60 + StartLane - EndLane) % 60 + 1;

            size = PassedThroughSeam ? 62 - size : size;
            return Math.Min(size, 60);
        }
    }

    public void Reset(int lane)
    {
        StartLane = lane;
        endLane = lane;
        PassedThroughSeam = false;
    }
}