using System;
using System.IO;
using SaturnEdit.Settings;
using SaturnView;

namespace SaturnEdit.Systems.Serialization;

public static class SettingsSerializer
{
    /// <summary>
    /// Converts <see cref="RenderSettings"/> into a string.
    /// </summary>
    /// <param name="settings">The <see cref="RenderSettings"/> to serialize.</param>
    public static string ToString(RenderSettings renderSettings)
    {
        return SettingsWriter.ToString(renderSettings);
    }
    
    /// <summary>
    /// Converts <see cref="EditorSettings"/> into a string.
    /// </summary>
    /// <param name="settings">The <see cref="EditorSettings"/> to serialize.</param>
    public static string ToString(EditorSettings editorSettings)
    {
        return SettingsWriter.ToString(editorSettings);
    }
    
    /// <summary>
    /// Converts <see cref="AudioSettings"/> into a string.
    /// </summary>
    /// <param name="settings">The <see cref="AudioSettings"/> to serialize.</param>
    public static string ToString(AudioSettings audioSettings)
    {
        return SettingsWriter.ToString(audioSettings);
    }
    
    /// <summary>
    /// Converts <see cref="ShortcutSettings"/> into a string.
    /// </summary>
    /// <param name="settings">The <see cref="ShortcutSettings"/> to serialize.</param>
    public static string ToString(ShortcutSettings shortcutSettings)
    {
        return SettingsWriter.ToString(shortcutSettings);
    }
    
    /// <summary>
    /// Reads a file and converts it into <see cref="RenderSettings"/>.
    /// </summary>
    /// <param name="path">The file to open.</param>
    public static RenderSettings ToRenderSettings(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            return SettingsReader.ToRenderSettings(lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new();
        }
    }
    
    /// <summary>
    /// Reads a file and converts it into <see cref="EditorSettings"/>.
    /// </summary>
    /// <param name="path">The file to open.</param>
    public static EditorSettings ToEditorSettings(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            return SettingsReader.ToEditorSettings(lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new();
        }
    }
    
    /// <summary>
    /// Reads a file and converts it into <see cref="AudioSettings"/>.
    /// </summary>
    /// <param name="path">The file to open.</param>
    public static AudioSettings ToAudioSettings(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            return SettingsReader.ToAudioSettings(lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new();
        }
    }
    
    /// <summary>
    /// Reads a file and converts it into <see cref="ShortcutSettings"/>.
    /// </summary>
    /// <param name="path">The file to open.</param>
    public static ShortcutSettings ToShortcutSettings(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            return SettingsReader.ToShortcutSettings(lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return new();
        }
    }
}