using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace SaturnEdit.Docking;

public enum TargetSide
{
    None = 0,
    Center = 1,
    Left = 2,
    Top = 3,
    Right = 4,
    Bottom = 5,
}

public partial class DockTarget : UserControl
{
    public DockTarget()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> UseOuterTargetsProperty = AvaloniaProperty.Register<DockTarget, bool>(nameof(UseOuterTargets), defaultValue: false);
    public bool UseOuterTargets
    {
        get => GetValue(UseOuterTargetsProperty);
        set
        {
            SetValue(UseOuterTargetsProperty, value);
            Update();
        }
    }

    public bool ShowSideTargets
    {
        get => showSideTargets;
        set
        {
            showSideTargets = value;
            Update();
        }
    }
    private bool showSideTargets = true;
    
    public bool ShowCenterTarget
    {
        get => showCenterTarget;
        set
        {
            showCenterTarget = value;
            Update();
        }
    }
    private bool showCenterTarget = true;

    public TargetSide TargetSide
    {
        get => targetSide;
        set
        {
            targetSide = value;
            Update();
        }
    }
    private TargetSide targetSide = TargetSide.None;

#region Methods
    public void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            TargetCenter.IsVisible = ShowCenterTarget;
            
            TargetOuterLeft.IsVisible   = UseOuterTargets && ShowSideTargets;
            TargetOuterTop.IsVisible    = UseOuterTargets && ShowSideTargets;
            TargetOuterRight.IsVisible  = UseOuterTargets && ShowSideTargets;
            TargetOuterBottom.IsVisible = UseOuterTargets && ShowSideTargets;
            
            TargetLeft.IsVisible   = !UseOuterTargets && ShowSideTargets;
            TargetTop.IsVisible    = !UseOuterTargets && ShowSideTargets;
            TargetRight.IsVisible  = !UseOuterTargets && ShowSideTargets;
            TargetBottom.IsVisible = !UseOuterTargets && ShowSideTargets;

            IndicatorCenter.IsVisible = TargetSide == TargetSide.Center;
            IndicatorLeft.IsVisible   = TargetSide == TargetSide.Left;
            IndicatorTop.IsVisible    = TargetSide == TargetSide.Top;
            IndicatorRight.IsVisible  = TargetSide == TargetSide.Right;
            IndicatorBottom.IsVisible = TargetSide == TargetSide.Bottom;
        });
    }

    private void HitTestTargets()
    {
        if (!IsVisible) return;
        if (DockArea.Instance == null) return;
        if (VisualRoot == null) return;

        if (!DockArea.Instance.WindowDragActive)
        {
            TargetSide = TargetSide.None;
            Update();
            
            return;
        }

        Rect bounds = DockArea.ScreenBounds(this);

        int x = DockArea.Instance.PointerPosition.X;
        int y = DockArea.Instance.PointerPosition.Y;

        bool left;
        bool right;
        bool top;
        bool bottom;
        
        if (ShowCenterTarget)
        {
            left   = x > bounds.Center.X - 20;
            right  = x < bounds.Center.X + 20;
            top    = y > bounds.Center.Y - 20;
            bottom = y < bounds.Center.Y + 20;

            if (left && right && top && bottom)
            {
                TargetSide = TargetSide.Center;
                return;
            }
        }

        if (showSideTargets)
        {
            if (UseOuterTargets)
            {
                // Left
                left   = x > bounds.Left;
                right  = x < bounds.Left + 40;
                top    = y > bounds.Center.Y - 20;
                bottom = y < bounds.Center.Y + 20;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Left;
                    return;
                }
                
                // Right
                left   = x > bounds.Right - 40;
                right  = x < bounds.Right;
                top    = y > bounds.Center.Y - 20;
                bottom = y < bounds.Center.Y + 20;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Right;
                    return;
                }
                
                // Top
                left   = x > bounds.Center.X - 20;
                right  = x < bounds.Center.X + 20;
                top    = y > bounds.Top;
                bottom = y < bounds.Top + 40;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Top;
                    return;
                }
                
                // Bottom
                left   = x > bounds.Center.X - 20;
                right  = x < bounds.Center.X + 20;
                top    = y > bounds.Bottom - 40;
                bottom = y < bounds.Bottom;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Bottom;
                    return;
                }
            }
            else
            {
                // Left
                left   = x > bounds.Center.X - 65;
                right  = x < bounds.Center.X - 25;
                top    = y > bounds.Center.Y - 20;
                bottom = y < bounds.Center.Y + 20;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Left;
                    return;
                }
                
                // Right
                left   = x > bounds.Center.X + 25;
                right  = x < bounds.Center.X + 65;
                top    = y > bounds.Center.Y - 20;
                bottom = y < bounds.Center.Y + 20;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Right;
                    return;
                }
                
                // Top
                left   = x > bounds.Center.X - 20;
                right  = x < bounds.Center.X + 20;
                top    = y > bounds.Center.Y - 65;
                bottom = y < bounds.Center.Y - 25;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Top;
                    return;
                }
                
                // Bottom
                left   = x > bounds.Center.X - 20;
                right  = x < bounds.Center.X + 20;
                top    = y > bounds.Center.Y + 25;
                bottom = y < bounds.Center.Y + 65;
                
                if (left && right && top && bottom)
                {
                    TargetSide = TargetSide.Bottom;
                    return;
                }
            }
        }

        TargetSide = TargetSide.None;
    }
#endregion Methods

#region System Event Handlers
    private void OnWindowDragStarted(object? sender, EventArgs e) => HitTestTargets();
    
    private void OnWindowDragged(object? sender, EventArgs e) => HitTestTargets();
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        Update();
        
        if (DockArea.Instance != null)
        {
            DockArea.Instance.WindowDragStarted += OnWindowDragStarted;
            DockArea.Instance.WindowDragged += OnWindowDragged;
        }
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (DockArea.Instance != null)
        {
            DockArea.Instance.WindowDragStarted -= OnWindowDragStarted;
            DockArea.Instance.WindowDragged -= OnWindowDragged;
        }
        
        base.OnUnloaded(e);
    }
#endregion UI Event Handlers
}