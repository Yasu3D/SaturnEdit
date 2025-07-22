using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace SaturnEdit.Views.Main.ChartEditor.Tabs;

public partial class NotePaletteView : UserControl
{
    public NotePaletteView()
    {
        InitializeComponent();
        UpdateIcons();
    }

    public void UpdateIcons()
    {
        bool useMonoIcons = false;
        
        int touchColorId = 0;
        int chainColorId = 1;
        int holdColorId = 6;
        int slideClockwiseColorId = 2;
        int slideCounterclockwiseColorId = 3;
        int snapForwardColorId = 4;
        int snapBackwardColorId = 5;

        if (useMonoIcons)
        {
            SvgTouch.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_touch.svg";
            SvgChain.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_chain.svg";
            SvgHold.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_hold.svg";
            SvgSlideClockwise.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_slide_clw.svg";
            SvgSlideCounterclockwise.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_slide_ccw.svg";
            SvgSnapForward.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_snap_fwd.svg";
            SvgSnapBackward.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_snap_bwd.svg";
            SvgLaneShow.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_lane_show.svg";
            SvgLaneHide.Path = "avares://SaturnEdit/Assets/Icons/Mono/icon_lane_hide.svg";
        }
        else
        {
            SvgTouch.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_touch_{touchColorId}.svg";
            SvgChain.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_chain_{chainColorId}.svg";
            SvgHold.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_hold_{holdColorId}.svg";
            SvgSlideClockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_clw_{slideClockwiseColorId}.svg";
            SvgSlideCounterclockwise.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_slide_ccw_{slideCounterclockwiseColorId}.svg";
            SvgSnapForward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_fwd_{snapForwardColorId}.svg";
            SvgSnapBackward.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_snap_bwd_{snapBackwardColorId}.svg";
            
            SvgLaneShow.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_lane_show.svg";
            SvgLaneHide.Path = $"avares://SaturnEdit/Assets/Icons/Color/icon_lane_hide.svg";
        }
    }

    private void GroupOnRightClick(object? sender, PointerPressedEventArgs e)
    {
        if (e.Properties.IsRightButtonPressed == false) return;

        if (Equals(sender, GroupTouch))
        {
            VisibilityToggleTouch.IsVisible = !VisibilityToggleTouch.IsVisible;
        }
        
        if (Equals(sender, GroupChain))
        {
            VisibilityToggleChain.IsVisible = !VisibilityToggleChain.IsVisible;
        }
        
        if (Equals(sender, GroupHold))
        {
            VisibilityToggleHold.IsVisible = !VisibilityToggleHold.IsVisible;
        }
        
        if (Equals(sender, GroupSlideClockwise))
        {
            VisibilityToggleSlideClockwise.IsVisible = !VisibilityToggleSlideClockwise.IsVisible;
        }
        
        if (Equals(sender, GroupSlideCounterclockwise))
        {
            VisibilityToggleSlideCounterclockwise.IsVisible = !VisibilityToggleSlideCounterclockwise.IsVisible;
        }
        
        if (Equals(sender, GroupSnapForward))
        {
            VisibilityToggleSnapForward.IsVisible = !VisibilityToggleSnapForward.IsVisible;
        }
        
        if (Equals(sender, GroupSnapBackward))
        {
            VisibilityToggleSnapBackward.IsVisible = !VisibilityToggleSnapBackward.IsVisible;
        }
        
        if (Equals(sender, GroupLaneShow))
        {
            VisibilityToggleLaneShow.IsVisible = !VisibilityToggleLaneShow.IsVisible;
        }
        
        if (Equals(sender, GroupLaneHide))
        {
            VisibilityToggleLaneHide.IsVisible = !VisibilityToggleLaneHide.IsVisible;
        }
    }
}