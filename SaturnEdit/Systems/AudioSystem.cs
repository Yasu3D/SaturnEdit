using System;
using ManagedBass;
using SaturnEdit.Audio;

namespace SaturnEdit.Systems;

public static class AudioSystem
{
    static AudioSystem()
    {
        Bass.Init(Flags: DeviceInitFlags.Latency);
        Bass.UpdatePeriod = 20;
        Bass.PlaybackBufferLength = 150;

        Bass.GetInfo(out BassInfo info);
        latency = info.Latency;
        
        audioChannel = new(@"X:\Wacca-Plus-Charts\v11 act 3\new\Galaxy Friends Inferno\galaxy_friends.wav");
        Console.WriteLine(audioChannel.SampleRate);
        
        
        //ChartSystem.EntryChanged += OnEntryChanged;
        //OnEntryChanged(null, EventArgs.Empty);
        //
        //SettingsSystem.SettingsChanged += OnSettingsChanged;
        //OnSettingsChanged(null, EventArgs.Empty);

        TimeSystem.PlaybackStateChanged += OnPlaybackStateChanged;
        OnPlaybackStateChanged(null, EventArgs.Empty);
        
        TimeSystem.PlaybackSpeedChanged += OnPlaybackSpeedChanged;
        OnPlaybackSpeedChanged(null, EventArgs.Empty);
        
        //TimeSystem.TimestampChanged += OnTimestampChanged;
        //OnTimestampChanged(null, EventArgs.Empty);
    }

    private static AudioChannel audioChannel;
    
    private static float latency = 0;
    private static float NormalizeVolume(float decibel) => (decibel + 60.0f) / 60.0f;

    public static void OnClosed(object? sender, EventArgs e)
    {
        Bass.Free();
    }
    
    /*
    private static void OnEntryChanged(object? sender, EventArgs e)
    {
        try
        {
            TimeSystem.PlaybackState = false;
            
            OnSettingsChanged(null, EventArgs.Empty);
            OnPlaybackSpeedChanged(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    */

    /*
    private static void OnSettingsChanged(object? sender, EventArgs e)
    {
        
    }
    */
    
    private static void OnPlaybackStateChanged(object? sender, EventArgs e)
    {
        Console.WriteLine(TimeSystem.PlaybackState);
        
        try
        {
            audioChannel.Playing = TimeSystem.PlaybackState;
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }
    
    private static void OnPlaybackSpeedChanged(object? sender, EventArgs e)
    {
        try
        {
            audioChannel.Speed = TimeSystem.PlaybackSpeed;
        }
        catch (Exception ex)
        {
            // don't throw
            Console.WriteLine(ex);
        }
    }
    
    /*
    private static void OnTimestampChanged(object? sender, EventArgs e)
    {
        
    }
    */
}