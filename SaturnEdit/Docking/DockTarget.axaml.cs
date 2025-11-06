using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
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
        Update();
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
    private void Update()
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
#endregion Methods
    
#region UI Event Delegates
    private void Target_OnPointerExited(object? sender, PointerEventArgs e)
    {
        TargetSide = TargetSide.None;
        Update();
    }
    
    private void TargetLeft_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        TargetSide = TargetSide.Left;
        Update();
    }

    private void TargetTop_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        TargetSide = TargetSide.Top;
        Update();
    }

    private void TargetRight_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        TargetSide = TargetSide.Right;
        Update();
    }

    private void TargetBottom_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        TargetSide = TargetSide.Bottom;
        Update();
    }

    private void TargetCenter_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        TargetSide = TargetSide.Center;
        Update();
    }
#endregion UI Event Delegates
}