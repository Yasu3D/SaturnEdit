using System;
using System.Buffers;
using ManagedBass;
using ManagedBass.Flac;
using ManagedBass.Fx;

namespace SaturnEdit.Audio;

public class AudioSample
{
    public AudioSample(string path)
    {
        StreamHandle = Bass.SampleLoad(path, 0, 0, 1, BassFlags.Default);
        
        // Explode if load failed.
        if (StreamHandle == 0) throw new("Audio file could not be loaded by Bass.");

        StreamHandle = Bass.SampleGetChannel(StreamHandle);
    }
    
    /// <summary>
    /// The handle of the Bass stream.
    /// </summary>
    public int StreamHandle { get; }

    /// <summary>
    /// The playback volume of the audio channel.<br/>Range 0 to 1
    /// </summary>
    public double Volume
    {
        get => Bass.ChannelGetAttribute(StreamHandle, ChannelAttribute.Volume);
        set => Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Volume, value);
    }

    /// <summary>
    /// The current level of the left channel.<br/>Range 0-1.
    /// </summary>
    public float LevelLeft
    {
        get
        {
            int level = Bass.ChannelGetLevelLeft(StreamHandle);
            return level == -1 ? -1 : level / 32768.0f;
        }
    }

    /// <summary>
    /// The current level of the right channel.<br/>Range 0-1.
    /// </summary>
    public float LevelRight
    {
        get
        {
            int level = Bass.ChannelGetLevelRight(StreamHandle);
            return level == -1 ? -1 : level / 32768.0f;
        }
    }
    
    public void Play()
    {
        Bass.ChannelPlay(StreamHandle, true);
    }

    public void Pause()
    {
        Bass.ChannelPause(StreamHandle);
    }
}