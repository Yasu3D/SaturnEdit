using System;
using System.IO;

namespace SaturnEdit.Utilities;

public static class PersistentDataPathHelper
{
    public static string PersistentDataPath => IsPortableInstall ? PortableDataDirectory : LocalApplicationDataDirectory;
    
    private static bool IsPortableInstall => Directory.Exists(PortableDataDirectory);
    
    private static string PortableDataDirectory => Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? "", "Portable");
    private static string LocalApplicationDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SaturnEdit");
}