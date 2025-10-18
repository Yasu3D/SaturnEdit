using SaturnData.Notation.Interfaces;

namespace SaturnEdit.Systems;

public static class EditorSystem
{
    public static void Initialize()
    {
        
    }

    public static ITimeable? PointerOverObject { get; set; } = null;
}