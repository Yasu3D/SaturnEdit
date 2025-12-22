using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SaturnEdit.Systems;

namespace SaturnEdit.Windows.Dialogs.Settings.Tabs;

public partial class SettingsAudioView : UserControl
{
    public SettingsAudioView()
    {
        InitializeComponent();
    }
    
    private bool blockEvents = false;
    
#region System Event Handlers
    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            blockEvents = true;
            
            NumericUpDownHoldLoopStart.Value = (decimal)SettingsSystem.AudioSettings.HoldLoopStart;
            NumericUpDownHoldLoopEnd.Value = (decimal)SettingsSystem.AudioSettings.HoldLoopEnd;
            
            blockEvents = false;
        });
    }
#endregion System Event Handlers

#region UI Event Handlers
    protected override void OnLoaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged += OnSettingsChanged;
        OnSettingsChanged(null, EventArgs.Empty);
        
        base.OnLoaded(e);
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        SettingsSystem.SettingsChanged -= OnSettingsChanged;
        
        base.OnUnloaded(e);
    }
    
    private void NumericUpDownHoldLoopStart_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopStart == null) return;

        SettingsSystem.AudioSettings.HoldLoopStart = (float?)NumericUpDownHoldLoopStart.Value ?? 0;
    }

    private void NumericUpDownHoldLoopEnd_OnValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (blockEvents) return;
        if (NumericUpDownHoldLoopEnd == null) return;
        
        SettingsSystem.AudioSettings.HoldLoopEnd = (float?)NumericUpDownHoldLoopEnd.Value ?? 0;
    }
#endregion UI Event Handlers
}