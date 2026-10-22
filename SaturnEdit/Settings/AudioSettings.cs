using System;
using SaturnData.Content.Items;
using SaturnData.Content.Items.Attributes;

namespace SaturnEdit.Settings;

public class AudioSettings : ContentItem
{
    public new event EventHandler? PropertyChanged;

    public event EventHandler? VolumeChanged;
    public event EventHandler? HitsoundsChanged;

#region Enum Definitions
    public enum QuantizationOption
    {
        Off = 0,
        Nearest = 1,
        Previous = 2,
        Next = 3,
    }
#endregion

    [SctLineBreak]
    [SctKey("LOOP_PLAYBACK", 10)]
    public bool LoopPlayback
    {
        get => loopPlayback;
        set
        {
            if (loopPlayback == value) return;

            loopPlayback = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool loopPlayback = false;

    [SctKey("METRONOME", 10)]
    public bool Metronome
    {
        get => metronome;
        set
        {
            if (metronome == value) return;

            metronome = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool metronome = false;

    [SctKey("QUANTIZED_PAUSE", 10)]
    public QuantizationOption QuantizedPause
    {
        get => quantizedPause;
        set
        {
            if (quantizedPause == value) return;

            quantizedPause = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private QuantizationOption quantizedPause = QuantizationOption.Off;

    [SctKey("QUANTIZED_SEEK", 10)]
    public QuantizationOption QuantizedSeek
    {
        get => quantizedSeek;
        set
        {
            if (quantizedSeek == value) return;

            quantizedSeek = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private QuantizationOption quantizedSeek = QuantizationOption.Off;

    [SctKey("LOOP_TO_START", 10)]
    public bool LoopToStart
    {
        get => loopToStart;
        set
        {
            if (loopToStart == value) return;

            loopToStart = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool loopToStart = false;

    [SctKey("VOLUME_MASTER", 10)]
    public int MasterVolume
    {
        get => masterVolume;
        set
        {
            if (masterVolume == value) return;

            masterVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int masterVolume = -30;

    [SctKey("VOLUME_AUDIO", 10)]
    public int AudioVolume
    {
        get => audioVolume;
        set
        {
            if (audioVolume == value) return;

            audioVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int audioVolume = 0;

    [SctKey("VOLUME_HITSOUND", 10)]
    public int HitsoundVolume
    {
        get => hitsoundVolume;
        set
        {
            if (hitsoundVolume == value) return;

            hitsoundVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int hitsoundVolume = 0;

    [SctKey("VOLUME_GUIDE", 10)]
    public int GuideVolume
    {
        get => guideVolume;
        set
        {
            if (guideVolume == value) return;

            guideVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int guideVolume = -20;

    [SctKey("VOLUME_TOUCH", 10)]
    public int TouchVolume
    {
        get => touchVolume;
        set
        {
            if (touchVolume == value) return;

            touchVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int touchVolume = 0;

    [SctKey("VOLUME_HOLD", 10)]
    public int HoldVolume
    {
        get => holdVolume;
        set
        {
            if (holdVolume == value) return;

            holdVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int holdVolume = 0;

    [SctKey("VOLUME_SLIDE", 10)]
    public int SlideVolume
    {
        get => slideVolume;
        set
        {
            if (slideVolume == value) return;

            slideVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int slideVolume = 0;

    [SctKey("VOLUME_BONUS", 10)]
    public int BonusVolume
    {
        get => bonusVolume;
        set
        {
            if (bonusVolume == value) return;

            bonusVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int bonusVolume = 0;

    [SctKey("VOLUME_R", 10)]
    public int RVolume
    {
        get => rVolume;
        set
        {
            if (rVolume == value) return;

            rVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int rVolume = 0;

    [SctKey("VOLUME_CLICK", 10)]
    public int StartClickVolume
    {
        get => startClickVolume;
        set
        {
            if (startClickVolume == value) return;

            startClickVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int startClickVolume = 0;

    [SctKey("VOLUME_METRONOME", 10)]
    public int MetronomeVolume
    {
        get => metronomeVolume;
        set
        {
            if (metronomeVolume == value) return;

            metronomeVolume = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private int metronomeVolume = 0;

    [SctKey("MUTE_MASTER", 10)]
    public bool MuteMaster
    {
        get => muteMaster;
        set
        {
            if (muteMaster == value) return;

            muteMaster = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteMaster = false;

    [SctKey("MUTE_AUDIO", 10)]
    public bool MuteAudio
    {
        get => muteAudio;
        set
        {
            if (muteAudio == value) return;

            muteAudio = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteAudio = false;

    [SctKey("MUTE_HITSOUND", 10)]
    public bool MuteHitsound
    {
        get => muteHitsound;
        set
        {
            if (muteHitsound == value) return;

            muteHitsound = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteHitsound = false;

    [SctKey("MUTE_GUIDE", 10)]
    public bool MuteGuide
    {
        get => muteGuide;
        set
        {
            if (muteGuide == value) return;

            muteGuide = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteGuide = false;

    [SctKey("MUTE_TOUCH", 10)]
    public bool MuteTouch
    {
        get => muteTouch;
        set
        {
            if (muteTouch == value) return;

            muteTouch = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteTouch = false;

    [SctKey("MUTE_HOLD", 10)]
    public bool MuteHold
    {
        get => muteHold;
        set
        {
            if (muteHold == value) return;

            muteHold = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteHold = false;

    [SctKey("MUTE_SLIDE", 10)]
    public bool MuteSlide
    {
        get => muteSlide;
        set
        {
            if (muteSlide == value) return;

            muteSlide = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteSlide = false;

    [SctKey("MUTE_BONUS", 10)]
    public bool MuteBonus
    {
        get => muteBonus;
        set
        {
            if (muteBonus == value) return;

            muteBonus = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteBonus = false;

    [SctKey("MUTE_R", 10)]
    public bool MuteR
    {
        get => muteR;
        set
        {
            if (muteR == value) return;

            muteR = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteR = false;

    [SctKey("MUTE_CLICK", 10)]
    public bool MuteStartClick
    {
        get => muteStartClick;
        set
        {
            if (muteStartClick == value) return;

            muteStartClick = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteStartClick = false;

    [SctKey("MUTE_METRONOME", 10)]
    public bool MuteMetronome
    {
        get => muteMetronome;
        set
        {
            if (muteMetronome == value) return;

            muteMetronome = value;
            PropertyChanged?.Invoke(this, EventArgs.Empty);
            VolumeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    private bool muteMetronome = false;

    [SctKey("SOUNDPACK", 10)]
    public int SelectedSoundpackIndex { get; set; }

    //public string NoteSoundId
    //{
    //    get => noteSoundId;
    //    set
    //    {
    //        if (noteSoundId == value) return;
//
    //        noteSoundId = value;
    //        PropertyChanged?.Invoke(null, EventArgs.Empty);
    //        HitsoundsChanged?.Invoke(null, EventArgs.Empty);
    //    }
    //}
    //private string noteSoundId = "";

    //public NoteSound SelectedNoteSound
    //{
    //    //get => throw new NotImplementedException();
    //}
}