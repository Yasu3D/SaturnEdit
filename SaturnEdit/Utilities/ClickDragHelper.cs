using System;
using Avalonia.Input;

namespace SaturnEdit.Utilities;

public class ClickDragHelper
{
    public PointerPoint? StartPoint { get; private set; } = null;

    public bool DragActive(PointerPoint other)
    {
        if (StartPoint == null) return false;

        double distance = Math.Max(Math.Abs(StartPoint.Value.Position.X - other.Position.X), Math.Abs(StartPoint.Value.Position.Y - other.Position.Y));
        return distance > 10;
    }
    
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

    public void Reset(PointerPoint? startPoint, int lane)
    {
        StartPoint = startPoint;
        StartLane = lane;
        endLane = lane;
        PassedThroughSeam = false;
    }
}