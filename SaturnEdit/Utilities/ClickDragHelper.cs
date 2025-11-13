using System;
using Avalonia.Input;

namespace SaturnEdit.Utilities;

public class ClickDragHelper
{
    public PointerPoint? StartPoint { get; private set; } = null;

    public PointerPoint? EndPoint
    {
        get => endPoint;
        set
        {
            endPoint = value;

            PassedMinDistance = PassedMinDistance || Distance > 5;
        }
    }

    private PointerPoint? endPoint = null;

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

    public bool IsActive
    {
        get
        {
            if (StartPoint == null) return false;
            if (EndPoint == null) return false;

            return PassedMinDistance;
        }
    }

    public int Tally { get; private set; } = 0;
    
    public double Distance
    {
        get
        {
            if (StartPoint == null) return 0;
            if (EndPoint == null) return 0;
            
            return Math.Max(Math.Abs(StartPoint.Value.Position.X - EndPoint.Value.Position.X), Math.Abs(StartPoint.Value.Position.Y - EndPoint.Value.Position.Y));
        }
    }
    
    public bool PassedThroughSeam { get; private set; } = false;
    
    public bool PassedMinDistance { get; private set; }  = false;

#region Methods
    public void Reset(PointerPoint? start, PointerPoint? end, int lane)
    {
        StartPoint = start;
        EndPoint = end;
        StartLane = lane;
        endLane = lane;
        Tally = 0;
        PassedThroughSeam = false;
        PassedMinDistance = false;
    }
#endregion Methods
}