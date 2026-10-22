using System.Collections.Generic;
using System.Text;
using SaturnEdit.Settings;
using SaturnView;

namespace SaturnEdit.Systems.Serialization;

public static class SettingsWriter
{
    public static string ToString(RenderSettings renderSettings)
    {
        StringBuilder sb = new();

        sb.Append($"{"@VERSION",-28}1\n");
        sb.Append('\n');

        sb.Append($"{"@LOW_PERFORMANCE",-28}{(renderSettings.LowPerformanceMode ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        
        sb.Append($"{"@NOTE_SPEED",     -28}{renderSettings.NoteSpeed}\n");
        sb.Append($"{"@JUDGELINE_COLOR",-28}{renderSettings.JudgementLineColor}\n");
        sb.Append($"{"@NOTE_THICKNESS", -28}{renderSettings.NoteThickness}\n");
        sb.Append('\n');
        
        sb.Append($"{"@LANE_TOGGLE_ANIM",    -28}{(renderSettings.ShowLaneToggleAnimations ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@HIDE_EVENTS_PLAY",    -28}{(renderSettings.HideEventMarkersDuringPlayback ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@HIDE_LANETOGGLE_PLAY", -28}{(renderSettings.HideLaneToggleNotesDuringPlayback ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@HIDE_HOLDPOINT_PLAY", -28}{(renderSettings.HideHoldControlPointsDuringPlayback ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@HIDE_BOOKMARK_PLAY",  -28}{(renderSettings.HideBookmarksDuringPlayback ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        
        sb.Append($"{"@TOUCH_COLOR",-28}{renderSettings.TouchNoteColor}\n");
        sb.Append($"{"@CHAIN_COLOR",-28}{renderSettings.ChainNoteColor}\n");
        sb.Append($"{"@HOLD_COLOR",-28}{renderSettings.HoldNoteColor}\n");
        sb.Append($"{"@SLIDECLW_COLOR",-28}{renderSettings.SlideClockwiseNoteColor}\n");
        sb.Append($"{"@SLIDECCW_COLOR",-28}{renderSettings.SlideCounterclockwiseNoteColor}\n");
        sb.Append($"{"@SNAPFWD_COLOR",-28}{renderSettings.SnapForwardNoteColor}\n");
        sb.Append($"{"@SNAPBWD_COLOR",-28}{renderSettings.SnapBackwardNoteColor}\n");
        sb.Append('\n');
        
        sb.Append($"{"@GUIDE_LINE",-28}{renderSettings.GuideLineType}\n");
        sb.Append($"{"@BACKGROUND_DIM",-28}{renderSettings.BackgroundDim}\n");
        sb.Append($"{"@HIDDEN_OPACITY",-28}{renderSettings.HiddenOpacity}\n");
        sb.Append('\n');
        
        sb.Append($"{"@SHOW_SPEED_CHANGE",-28}{(renderSettings.ShowSpeedChanges ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_VIS_CHANGE",-28}{(renderSettings.ShowVisibilityChanges ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@VISUALIZE_SWEEP",-28}{(renderSettings.VisualizeLaneSweeps ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        
        sb.Append($"{"@SHOW_JUDGE_AREAS",-28}{(renderSettings.ShowJudgeAreas ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_MARVELOUS",-28}{(renderSettings.ShowMarvelousArea ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_GREAT",-28}{(renderSettings.ShowGreatArea ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_GOOD",-28}{(renderSettings.ShowGoodArea ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SATURN_JUDGE",-28}{(renderSettings.SaturnJudgeAreas ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        
        sb.Append($"{"@SHOW_TOUCH",-28}{(renderSettings.ShowTouchNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_CHAIN",-28}{(renderSettings.ShowChainNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_HOLD",-28}{(renderSettings.ShowHoldNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_SLIDECLOCKWISE",-28}{(renderSettings.ShowSlideClockwiseNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_SLIDECOUNTERCLOCKWISE",-28}{(renderSettings.ShowSlideCounterclockwiseNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_SNAPFORWARD",-28}{(renderSettings.ShowSnapForwardNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_SNAPBACKWARD",-28}{(renderSettings.ShowSnapBackwardNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_SYNC",-28}{(renderSettings.ShowSyncNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_MEASURELINE",-28}{(renderSettings.ShowMeasureLineNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_BEATLINE",-28}{(renderSettings.ShowBeatLineNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_LANESHOW",-28}{(renderSettings.ShowLaneShowNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_LANEHIDE",-28}{(renderSettings.ShowLaneHideNotes ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_TEMPOCHANGE",-28}{(renderSettings.ShowTempoChangeEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_METRECHANGE",-28}{(renderSettings.ShowMetreChangeEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_SPEEDCHANGE",-28}{(renderSettings.ShowSpeedChangeEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_VISIBILITYCHANGE",-28}{(renderSettings.ShowVisibilityChangeEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_REVERSEEFFECT",-28}{(renderSettings.ShowReverseEffectEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_STOPEFFECT",-28}{(renderSettings.ShowStopEffectEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_TUTORIALMARKER",-28}{(renderSettings.ShowTutorialMarkerEvents ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@SHOW_BOOKMARK",-28}{(renderSettings.ShowBookmarks ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        
        sb.Append($"{"@RNOTE_EFFECT_VISIBILITY",-28}{renderSettings.RNoteEffectVisibility}\n");
        sb.Append($"{"@BONUS_EFFECT_VISIBILITY",-28}{renderSettings.BonusEffectVisibility}\n");
        sb.Append($"{"@RNOTE_EFFECT_OPACITY",-28}{renderSettings.RNoteEffectOpacity}\n");
        sb.Append($"{"@CLEAR_BG_VISIBILITY",-28}{renderSettings.ClearBackgroundVisibility}\n");
        sb.Append($"{"@LEVEL_VISIBILITY",-28}{renderSettings.LevelDisplayVisibility}\n");
        sb.Append($"{"@TITLE_VISIBILITY",-28}{renderSettings.TitleDisplayVisibility}\n");
        sb.Append($"{"@DIFFICULTY_VISIBILITY",-28}{renderSettings.DifficultyDisplayVisibility}\n");
        sb.Append($"{"@TICK_VISIBILITY",-28}{renderSettings.UnitTickVisibility}\n");

        return sb.ToString();
    }

    public static string ToString(EditorSettings editorSettings)
    {
        StringBuilder sb = new();

        sb.Append($"{"@VERSION",-28}1\n");
        sb.Append('\n');
        
        sb.Append($"{"@LOCALE", -28}{editorSettings.Locale}\n");
        sb.Append($"{"@THEME", -28}{editorSettings.Theme}\n");
        sb.Append($"{"@SHOW_SPLASH_SCREEN", -28}{(editorSettings.ShowSplashScreen ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@CONTINUE_LAST_SESSION", -28}{(editorSettings.ContinueLastSession ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@CHECK_FOR_UPDATES", -28}{(editorSettings.CheckForUpdates ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@LAST_SESSION_PATH", -28}{editorSettings.LastSessionPath}\n");
        sb.Append($"{"@TXT_SPACES", -28}{(editorSettings.ChartViewTxtShowSpaces ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@TXT_SYNTAX", -28}{(editorSettings.ChartViewTxtSyntaxHighlighting ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@AUTO_SAVE", -28}{editorSettings.AutoSaveFrequency}\n");
        sb.Append($"{"@CLICK_DRAG", -28}{editorSettings.ClickDragThreshold}\n");
        sb.Append('\n');

        sb.Append("@RECENT_FILES\n");
        foreach (string file in editorSettings.RecentChartFiles)
        {
            sb.Append($"    {file}\n");
        }

        return sb.ToString();
    }

    public static string ToString(AudioSettings audioSettings)
    {
        StringBuilder sb = new();

        sb.Append($"{"@VERSION",-28}1\n");
        sb.Append('\n');
        
        sb.Append($"{"@LOOP_PLAYBACK", -28}{(audioSettings.LoopPlayback ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@METRONOME", -28}{(audioSettings.Metronome ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@QUANTIZED_PAUSE", -28}{audioSettings.QuantizedPause}\n");
        sb.Append($"{"@QUANTIZED_SEEK", -28}{audioSettings.QuantizedSeek}\n");
        sb.Append($"{"@LOOP_TO_START", -28}{(audioSettings.LoopToStart ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        sb.Append($"{"@VOLUME_MASTER", -28}{audioSettings.MasterVolume}\n");
        sb.Append($"{"@VOLUME_AUDIO", -28}{audioSettings.AudioVolume}\n");
        sb.Append($"{"@VOLUME_HITSOUND", -28}{audioSettings.HitsoundVolume}\n");
        sb.Append($"{"@VOLUME_GUIDE", -28}{audioSettings.GuideVolume}\n");
        sb.Append($"{"@VOLUME_TOUCH", -28}{audioSettings.TouchVolume}\n");
        sb.Append($"{"@VOLUME_HOLD", -28}{audioSettings.HoldVolume}\n");
        sb.Append($"{"@VOLUME_SLIDE", -28}{audioSettings.SlideVolume}\n");
        sb.Append($"{"@VOLUME_BONUS", -28}{audioSettings.BonusVolume}\n");
        sb.Append($"{"@VOLUME_R", -28}{audioSettings.RVolume}\n");
        sb.Append($"{"@VOLUME_CLICK", -28}{audioSettings.StartClickVolume}\n");
        sb.Append($"{"@VOLUME_METRONOME", -28}{audioSettings.MetronomeVolume}\n");
        sb.Append('\n');
        sb.Append($"{"@MUTE_MASTER", -28}{(audioSettings.MuteMaster ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_AUDIO", -28}{(audioSettings.MuteAudio ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_HITSOUND", -28}{(audioSettings.MuteHitsound ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_GUIDE", -28}{(audioSettings.MuteGuide ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_TOUCH", -28}{(audioSettings.MuteTouch ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_HOLD", -28}{(audioSettings.MuteHold ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_SLIDE", -28}{(audioSettings.MuteSlide ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_BONUS", -28}{(audioSettings.MuteBonus ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_R", -28}{(audioSettings.MuteR ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_CLICK", -28}{(audioSettings.MuteStartClick ? "TRUE" : "FALSE")}\n");
        sb.Append($"{"@MUTE_METRONOME", -28}{(audioSettings.MuteMetronome ? "TRUE" : "FALSE")}\n");
        sb.Append('\n');
        //sb.Append($"{"@NOTE_SOUNDS", -28}{audioSettings.NoteSoundId}\n");

        return sb.ToString();
    }

    public static string ToString(ShortcutSettings shortcutSettings)
    {
        StringBuilder sb = new();

        sb.Append($"{"@VERSION",-28}1\n");
        sb.Append('\n');
        sb.Append("@SHORTCUTS\n");

        foreach (KeyValuePair<string, Shortcut> shortcut in shortcutSettings.Shortcuts)
        {
            sb.Append($"    {shortcut.Key,-60} {shortcut.Value.Key,-28} {(shortcut.Value.Control ? "C" : "_")} {(shortcut.Value.Alt ? "A" : "_")} {(shortcut.Value.Shift ? "S" : "_")}\n");
        }

        return sb.ToString();
    }
}