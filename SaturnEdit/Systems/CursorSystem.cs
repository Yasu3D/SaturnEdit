using System;
using SaturnData.Notation.Core;
using SaturnData.Notation.Interfaces;
using SaturnData.Notation.Notes;

namespace SaturnEdit.Systems;

public static class CursorSystem
{
    public static void Initialize() { }
    
    public static event EventHandler? TypeChanged;

    public static Note CursorNote
    {
        get => cursorNote;
        set
        {
            if (cursorNote == value) return;
            
            cursorNote = value;
            TypeChanged?.Invoke(null, EventArgs.Empty);
        }
    }
    private static Note cursorNote = new TouchNote(Timestamp.Zero, 30, 15, BonusType.Normal, JudgementType.Normal);

    public static int BackupPosition { get; set; } = 30;
    public static int BackupSize { get; set; } = 15;
}