using System;
using System.Buffers;
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
        if (StreamHandle == 0) throw new("Audio file could not be loaded by Bass.");

        StreamHandle = BassFx.TempoCreate(StreamHandle, BassFlags.FxFreeSource);
        
        // Explode if fx load failed.
        if (StreamHandle == 0) throw new("Audio file could not be loaded by BassFX.");

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
        get => Bass.ChannelBytes2Seconds(StreamHandle, Bass.ChannelGetPosition(StreamHandle)) * 1000;
        set => Bass.ChannelSetPosition(StreamHandle, Bass.ChannelSeconds2Bytes(StreamHandle, value * 0.001));
    }

    /// <summary>
    /// The playback volume of the audio channel.<br/>Range 0 to 1
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
    /// [5% - 300%]
    /// </remarks>
    public float Speed
    {
        get => speed;
        set
        {
            speed = Math.Clamp(value, 5, 300);
            Bass.ChannelSetAttribute(StreamHandle, ChannelAttribute.Frequency, speed * 0.01f * SampleRate);
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
            
            if (playing)
            {
                Bass.ChannelPlay(StreamHandle);
            }
            else
            {
                Bass.ChannelPause(StreamHandle);
            }
        }
    }
    private bool playing = false;

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

    public static float[]? GetWaveformData(string path)
    {
        // This code is borrowed/adapted from osu.
        
        const float resolution = 0.001f;
        const int pointsPerIteration = 1000;
        const int bytesPerSample = 4;

        float[]? points = null;
        float[]? sampleBuffer = null;
        
        try
        {
            // Try loading as flac.
            int handle = BassFlac.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan | BassFlags.Float);
            
            // Try loading as anything else if that failed.
            if (handle == 0 && Bass.LastError == Errors.FileFormat)
            {
                handle = Bass.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan | BassFlags.Float);
            }
            
            if (!Bass.ChannelGetInfo(handle, out ChannelInfo info)) return points;

            long length = Bass.ChannelGetLength(handle);

            // Each "amplitude point" is generated from a number of samples, each sample contains a number of channels
            int samplesPerPoint = (int)(info.Frequency * resolution * info.Channels);

            int bytesPerPoint = samplesPerPoint * bytesPerSample;

            int pointCount = (int)(length / bytesPerPoint);

            points = new float[pointCount];

            // Each iteration pulls in several samples
            int bytesPerIteration = bytesPerPoint * pointsPerIteration;

            sampleBuffer = ArrayPool<float>.Shared.Rent(bytesPerIteration / bytesPerSample);

            int pointIndex = 0;

            // Read sample data
            while (length > 0)
            {
                length = Bass.ChannelGetData(handle, sampleBuffer, bytesPerIteration);

                if (length < 0) return points;

                int samplesRead = (int)(length / bytesPerSample);

                // Each point is composed of multiple samples
                for (int i = 0; i < samplesRead && pointIndex < pointCount; i += samplesPerPoint)
                {
                    // Channels are interleaved in the sample data (data[0] -> channel0, data[1] -> channel1, data[2] -> channel0, etc)
                    // samplesPerPoint assumes this interleaving behaviour
                    int secondChannelIndex = info.Channels > 1 ? 1 : 0;
                 
                    float amplitude = 0;
                    
                    for (int j = i; j < i + samplesPerPoint; j += info.Channels)
                    {
                        // Find max amplitude in samples of either channel.
                        amplitude = Math.Max(amplitude, Math.Abs(sampleBuffer[j]));
                        amplitude = Math.Max(amplitude, Math.Abs(sampleBuffer[j + secondChannelIndex]));
                    }

                    // BASS may provide unclipped samples, so clip them ourselves
                    amplitude = Math.Min(1, amplitude);

                    points[pointIndex] = amplitude;
                    pointIndex++;
                }
            }
        }
        finally
        {
            if (sampleBuffer != null) ArrayPool<float>.Shared.Return(sampleBuffer);
        }

        return points;
    }

    public static double DecibelToVolume(int decibel)
    {
        double normalized = decibel / 60.0 + 1;
        double scaled = 0.1 * Math.Pow(Math.E, 2.4 * normalized) - 0.1;
        
        return scaled;
    }
}