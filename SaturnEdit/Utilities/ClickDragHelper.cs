using System;
using Avalonia.Input;

namespace SaturnEdit.Utilities;

public class ClickDragHelper
{
    public PointerPoint? StartPoint { get; private set; } = null;
    public PointerPoint? EndPoint { get; set; } = null;

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

            int offset = value - endLane;

            if (offset > 30)
            {
                offset -= 60;
            }
            else if (offset < -30)
            {
                offset += 60;
            }
            
            Tally += offset;
            
            endLane = value;
        }
    }
    private int endLane = 0;
    
    public bool PassedThroughSeam { get; private set; } = false;

    public int Position
    {
        get
        {
            if (PassedThroughSeam)
            {
                return Math.Max(StartLane, EndLane);
            }

            return Math.Min(StartLane, EndLane);
        }
    }

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

    public bool IsDragActive
    {
        get
        {
            if (StartPoint == null) return false;
            if (EndPoint == null) return false;

            double distance = Math.Max(Math.Abs(StartPoint.Value.Position.X - EndPoint.Value.Position.X), Math.Abs(StartPoint.Value.Position.Y - EndPoint.Value.Position.Y));
            return distance > 5;
        }
    }

    public int Tally { get; private set; } = 0;

#region Methods
    public void Reset(PointerPoint? startPoint, PointerPoint? endPoint, int lane)
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
        StartLane = lane;
        endLane = lane;
        PassedThroughSeam = false;
        Tally = 0;
    }
#endregion Methods
}