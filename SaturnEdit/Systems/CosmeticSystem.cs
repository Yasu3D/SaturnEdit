using System;
using System.IO;
using SaturnData.Content.Cosmetics;
using Tomlyn;
using ConsoleColor = SaturnData.Content.Cosmetics.ConsoleColor;

namespace SaturnEdit.Systems;

public enum CosmeticType
{
    ConsoleColor = 0,
    Emblem = 1,
    Icon = 2,
    Navigator = 3,
    NoteSound = 4,
    Plate = 5,
    SystemMusic = 6,
    SystemSound = 7,
    Title = 8,
}

public class CosmeticSystem
{
    public static event EventHandler? CosmeticLoaded;
    
    /// <summary>
    /// The cosmetic being edited/displayed.
    /// </summary>
    public static CosmeticItem CosmeticItem { get; private set; } = new Icon { Id = Guid.NewGuid().ToString() };
    
    /// <summary>
    /// Determines if the editor will prompt the user to save when a cosmetic is closed.
    /// </summary>
    public static bool IsSaved { get; private set; } = true;
    
#region Methods
    /// <summary>
    /// Creates a new cosmetic to work on by resetting the <see cref="CosmeticItem"/>, then invokes <see cref="CosmeticLoaded"/>.
    /// </summary>
    /// <param name="cosmeticType">The type of cosmetic to create.</param>
    public static void NewCosmetic(CosmeticType cosmeticType)
    {
        CosmeticItem = cosmeticType switch
        {
            CosmeticType.ConsoleColor => new ConsoleColor(),
            CosmeticType.Emblem       => new Emblem(),
            CosmeticType.Icon         => new Icon(),
            CosmeticType.Navigator    => new Navigator(),
            CosmeticType.NoteSound    => new NoteSound(),
            CosmeticType.Plate        => new Plate(),
            CosmeticType.SystemMusic  => new SystemMusic(),
            CosmeticType.SystemSound  => new SystemSound(),
            CosmeticType.Title        => new Title(),
            _ => throw new ArgumentOutOfRangeException(nameof(cosmeticType), cosmeticType, null),
        };
        
        CosmeticItem.Id = Guid.NewGuid().ToString();
        
        CosmeticLoaded?.Invoke(null, EventArgs.Empty);
        
        IsSaved = true;
    }

    /// <summary>
    /// Creates a new cosmetic to work on by reading data from a file, then invokes <see cref="CosmeticLoaded"/>.
    /// </summary>
    /// <param name="path">Path to the file to read from.</param>
    /// <param name="cosmeticType">The type of cosmetic to read.</param>
    public static void ReadCosmetic(string path, CosmeticType type)
    {
        try
        {
            string data = File.ReadAllText(path);

            CosmeticItem = type switch
            {
                CosmeticType.ConsoleColor => Toml.ToModel<ConsoleColor>(data),
                CosmeticType.Emblem       => Toml.ToModel<Emblem>(data),
                CosmeticType.Icon         => Toml.ToModel<Icon>(data),
                CosmeticType.Navigator    => Toml.ToModel<Navigator>(data),
                CosmeticType.NoteSound    => Toml.ToModel<NoteSound>(data),
                CosmeticType.Plate        => Toml.ToModel<Plate>(data),
                CosmeticType.SystemMusic  => Toml.ToModel<SystemMusic>(data),
                CosmeticType.SystemSound  => Toml.ToModel<SystemSound>(data),
                CosmeticType.Title        => Toml.ToModel<Title>(data),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
            CosmeticItem.AbsoluteSourcePath = path;
            
            CosmeticLoaded?.Invoke(null, EventArgs.Empty);
            
            IsSaved = true;
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
        }
    }
    
    /// <summary>
    /// Writes a cosmetic to a file.
    /// </summary>
    /// <param name="path">Path to the file to write to.</param>
    /// <param name="markAsSaved">Should the cosmetic be marked as saved?</param>
    /// <param name="updatePath">Should the <see cref="CosmeticItem.AbsoluteSourcePath"/> get updated?</param>
    public static bool WriteCosmetic(string path, bool markAsSaved, bool updatePath)
    {
        try
        {
            string data = Toml.FromModel(CosmeticItem);
            File.WriteAllText(path, data);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            return false;
        }
        
        if (updatePath)
        {
            CosmeticItem.AbsoluteSourcePath = path;
        }
        
        IsSaved = markAsSaved || IsSaved;
        return true;
    }
#endregion Methods
}