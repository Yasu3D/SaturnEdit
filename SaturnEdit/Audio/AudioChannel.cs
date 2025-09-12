using ManagedBass;
using ManagedBass.Flac;
using ManagedBass.Fx;

namespace SaturnEdit.Audio;

public class AudioChannel
{
    public AudioChannel(string path)
    {
        // Try loading as flac.
        StreamHandle = BassFlac.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan);
        
        // Try loading as anything else if that failed.
        if (StreamHandle == 0 && Bass.LastError == Errors.FileFormat)
        {
            StreamHandle = Bass.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan);
        }
        
        // Explode if load failed.
        if (StreamHandle == 0) throw new($"Audio file could not be loaded by Bass.");

        StreamHandle = BassFx.TempoCreate(StreamHandle, BassFlags.FxFreeSource);
        
        // Explode if fx load failed.
        if (StreamHandle == 0) throw new($"Audio file could not be loaded by BassFX.");

        // Prevent click noise when changing tempo.
        Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.TempoPreventClick, 1);
        
        Length = Bass.ChannelBytes2Seconds(StreamHandle, Bass.ChannelGetLength(StreamHandle)) * 1000;
        SampleRate = (int)Bass.ChannelGetAttribute(StreamHandle, ChannelAttribute.Frequency);
    }

    /// <summary>
    /// The handle of the Bass stream.
    /// </summary>
    public int StreamHandle { get; }
   
    /// <summary>
    /// The sample rate of the audio channel.
    /// </summary>
    public int SampleRate { get; }
    
    /// <summary>
    /// The length of the audio channel in milliseconds.
    /// </summary>
    public double Length { get; }
    
    /// <summary>
    /// The position of the audio channel in milliseconds.
    /// </summary>
    public double Position 
    { 
        get => Bass.ChannelBytes2Seconds(StreamHandle, Bass.ChannelGetLength(StreamHandle)) * 1000;
        set => Bass.ChannelSetPosition(StreamHandle, Bass.ChannelSeconds2Bytes(StreamHandle, value * 0.001));
    }

    /// <summary>
    /// The playback volume of the audio channel.
    /// </summary>
    public double Volume
    {
        get => Bass.ChannelGetAttribute(StreamHandle, ChannelAttribute.Volume);
        set => Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Volume, value);
    }

    /// <summary>
    /// The playback speed of the audio channel.
    /// </summary>
    /// <remarks>
    /// [0 - 200]
    /// </remarks>
    public float Speed
    {
        get => speed;
        set
        {
            speed = value;

            float rate = value * 0.01f;
            rate *= SampleRate;
            
            

            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Frequency, rate);
            
            float tempo = speed - 100;
            float pitch = tempo * 0.24f;
            //Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Pitch, pitch);
            //Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Tempo, tempo);
        }
    }
    private float speed = 100;

    /// <summary>
    /// The playback state of the audio channel.
    /// </summary>
    public bool Playing
    {
        get => playing;
        set
        {
            playing = value;

            if (playing) Bass.ChannelPlay(StreamHandle);
            else Bass.ChannelPause(StreamHandle);
        }
    }
    private bool playing = false;
}