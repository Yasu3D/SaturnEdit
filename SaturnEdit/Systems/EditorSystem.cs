namespace SaturnEdit.Systems;

public static class EditorSystem
{
    public static void Initialize() { }
    
#region Methods
    public static void Cut()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
    }

    public static void Copy()
    {
        if (SelectionSystem.SelectedObjects.Count == 0) return;
    }

    public static void Paste()
    {
        
    }
#endregion Methods
}