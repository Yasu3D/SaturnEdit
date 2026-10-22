using System;
using System.Globalization;
using SaturnData.Utilities;
using SaturnEdit.Settings;
using SaturnView;

namespace SaturnEdit.Systems.Serialization;

public static class SettingsReader
{
    public static RenderSettings ToRenderSettings(string[] lines)
    {
        RenderSettings renderSettings = new();
        
        foreach (string line in lines)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith('#')) continue;

                string value;
                if      (SerializationHelpers.ContainsKey(line, "@LOW_PERFORMANCE", out value)) { renderSettings.LowPerformanceMode = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@NOTE_SPEED", out value)) { renderSettings.NoteSpeed = Convert.ToInt32(value, CultureInfo.InvariantCulture); }
                else if (SerializationHelpers.ContainsKey(line, "@JUDGELINE_COLOR", out value)) { renderSettings.JudgementLineColor = toEnum<RenderSettings.JudgementLineColorOption>(value) ?? renderSettings.JudgementLineColor; }
                else if (SerializationHelpers.ContainsKey(line, "@NOTE_THICKNESS", out value)) { renderSettings.NoteThickness = toEnum<RenderSettings.NoteThicknessOption>(value) ?? renderSettings.NoteThickness; }

                else if (SerializationHelpers.ContainsKey(line, "@LANE_TOGGLE_ANIM", out value)) { renderSettings.ShowLaneToggleAnimations = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@HIDE_EVENTS_PLAY", out value)) { renderSettings.HideEventMarkersDuringPlayback = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@HIDE_LANETOGGLE_PLAY", out value)) { renderSettings.HideLaneToggleNotesDuringPlayback = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@HIDE_HOLDPOINT_PLAY", out value)) { renderSettings.HideHoldControlPointsDuringPlayback = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@HIDE_BOOKMARK_PLAY", out value)) { renderSettings.HideBookmarksDuringPlayback = value == "TRUE"; }

                else if (SerializationHelpers.ContainsKey(line, "@TOUCH_COLOR", out value)) { renderSettings.TouchNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.TouchNoteColor; }
                else if (SerializationHelpers.ContainsKey(line, "@CHAIN_COLOR", out value)) { renderSettings.ChainNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.ChainNoteColor; }
                else if (SerializationHelpers.ContainsKey(line, "@HOLD_COLOR", out value)) { renderSettings.HoldNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.HoldNoteColor; }
                else if (SerializationHelpers.ContainsKey(line, "@SLIDECLW_COLOR", out value)) { renderSettings.SlideClockwiseNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.SlideClockwiseNoteColor; }
                else if (SerializationHelpers.ContainsKey(line, "@SLIDECCW_COLOR", out value)) { renderSettings.SlideCounterclockwiseNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.SlideCounterclockwiseNoteColor; }
                else if (SerializationHelpers.ContainsKey(line, "@SNAPFWD_COLOR", out value)) { renderSettings.SnapForwardNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.SnapForwardNoteColor; }
                else if (SerializationHelpers.ContainsKey(line, "@SNAPBWD_COLOR", out value)) { renderSettings.SnapBackwardNoteColor = toEnum<RenderSettings.NoteColorOption>(value) ?? renderSettings.SnapBackwardNoteColor; }

                else if (SerializationHelpers.ContainsKey(line, "@GUIDE_LINE", out value)) { renderSettings.GuideLineType = toEnum<RenderSettings.GuideLineTypeOption>(value) ?? renderSettings.GuideLineType; }
                else if (SerializationHelpers.ContainsKey(line, "@BACKGROUND_DIM", out value)) { renderSettings.BackgroundDim = toEnum<RenderSettings.BackgroundDimOption>(value) ?? renderSettings.BackgroundDim; }
                else if (SerializationHelpers.ContainsKey(line, "@HIDDEN_OPACITY", out value)) { renderSettings.HiddenOpacity = Convert.ToInt32(value, CultureInfo.InvariantCulture); }

                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SPEED_CHANGE", out value)) { renderSettings.ShowSpeedChanges = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_VIS_CHANGE", out value)) { renderSettings.ShowVisibilityChanges = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@VISUALIZE_SWEEP", out value)) { renderSettings.VisualizeLaneSweeps = value == "TRUE"; }

                else if (SerializationHelpers.ContainsKey(line, "@SHOW_JUDGE_AREAS", out value)) { renderSettings.ShowJudgeAreas = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_MARVELOUS", out value)) { renderSettings.ShowMarvelousArea = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_GREAT", out value)) { renderSettings.ShowGreatArea = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_GOOD", out value)) { renderSettings.ShowGoodArea = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SATURN_JUDGE", out value)) { renderSettings.SaturnJudgeAreas = value == "TRUE"; }

                else if (SerializationHelpers.ContainsKey(line, "@SHOW_TOUCH", out value)) { renderSettings.ShowTouchNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_CHAIN", out value)) { renderSettings.ShowChainNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_HOLD", out value)) { renderSettings.ShowHoldNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SLIDECLOCKWISE", out value)) { renderSettings.ShowSlideClockwiseNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SLIDECOUNTERCLOCKWISE", out value)) { renderSettings.ShowSlideCounterclockwiseNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SNAPFORWARD", out value)) { renderSettings.ShowSnapForwardNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SNAPBACKWARD", out value)) { renderSettings.ShowSnapBackwardNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SYNC", out value)) { renderSettings.ShowSyncNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_MEASURELINE", out value)) { renderSettings.ShowMeasureLineNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_BEATLINE", out value)) { renderSettings.ShowBeatLineNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_LANESHOW", out value)) { renderSettings.ShowLaneShowNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_LANEHIDE", out value)) { renderSettings.ShowLaneHideNotes = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_TEMPOCHANGE", out value)) { renderSettings.ShowTempoChangeEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_METRECHANGE", out value)) { renderSettings.ShowMetreChangeEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_SPEEDCHANGE", out value)) { renderSettings.ShowSpeedChangeEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_VISIBILITYCHANGE", out value)) { renderSettings.ShowVisibilityChangeEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_REVERSEEFFECT", out value)) { renderSettings.ShowReverseEffectEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_STOPEFFECT", out value)) { renderSettings.ShowStopEffectEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_TUTORIALMARKER", out value)) { renderSettings.ShowTutorialMarkerEvents = value == "TRUE"; }
                else if (SerializationHelpers.ContainsKey(line, "@SHOW_BOOKMARK", out value)) { renderSettings.ShowBookmarks = value == "TRUE"; }
                
                else if (SerializationHelpers.ContainsKey(line, "@RNOTE_EFFECT_VISIBILITY", out value)) { renderSettings.RNoteEffectVisibility = toEnum<RenderSettings.EffectVisibilityOption>(value) ?? renderSettings.RNoteEffectVisibility; }
                else if (SerializationHelpers.ContainsKey(line, "@BONUS_EFFECT_VISIBILITY", out value)) { renderSettings.BonusEffectVisibility = toEnum<RenderSettings.EffectVisibilityOption>(value) ?? renderSettings.BonusEffectVisibility; }
                else if (SerializationHelpers.ContainsKey(line, "@RNOTE_EFFECT_OPACITY", out value)) { renderSettings.RNoteEffectOpacity = Convert.ToInt32(value, CultureInfo.InvariantCulture); }
                else if (SerializationHelpers.ContainsKey(line, "@CLEAR_BG_VISIBILITY", out value)) { renderSettings.ClearBackgroundVisibility = toEnum<RenderSettings.ClearBackgroundVisibilityOption>(value) ?? renderSettings.ClearBackgroundVisibility; }
                else if (SerializationHelpers.ContainsKey(line, "@LEVEL_VISIBILITY", out value)) { renderSettings.LevelDisplayVisibility = toEnum<RenderSettings.InterfaceVisibilityOption>(value) ?? renderSettings.LevelDisplayVisibility; }
                else if (SerializationHelpers.ContainsKey(line, "@TITLE_VISIBILITY", out value)) { renderSettings.TitleDisplayVisibility = toEnum<RenderSettings.InterfaceVisibilityOption>(value) ?? renderSettings.TitleDisplayVisibility; }
                else if (SerializationHelpers.ContainsKey(line, "@DIFFICULTY_VISIBILITY", out value)) { renderSettings.DifficultyDisplayVisibility = toEnum<RenderSettings.InterfaceVisibilityOption>(value) ?? renderSettings.DifficultyDisplayVisibility; }
                else if (SerializationHelpers.ContainsKey(line, "@TICK_VISIBILITY", out value)) { renderSettings.UnitTickVisibility = toEnum<RenderSettings.EffectVisibilityOption>(value) ?? renderSettings.UnitTickVisibility; }
            }
            catch (Exception ex)
            {
                // Don't throw.
                Console.WriteLine(ex);
            }
        }

        return renderSettings;

        T? toEnum<T>(string value) where T : struct
        {
            if (Enum.TryParse(value, out T result)) return result;

            return null;
        }
    }

    public static EditorSettings ToEditorSettings(string[] lines)
    {
        EditorSettings editorSettings = new();
        
        foreach (string line in lines)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith('#')) continue;

                string value;
                // if (SerializationHelpers.ContainsKey(line, "@ID ",   out value)) { contentItem.Id = value; }
                // else if (SerializationHelpers.ContainsKey(line, "@NAME ", out value)) { contentItem.Name = value; }
            }
            catch (Exception ex)
            {
                // Don't throw.
                Console.WriteLine(ex);
            }
        }

        return editorSettings;
    }

    public static AudioSettings ToAudioSettings(string[] lines)
    {
        AudioSettings audioSettings = new();
        
        foreach (string line in lines)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith('#')) continue;

                string value;
                // if (SerializationHelpers.ContainsKey(line, "@ID ",   out value)) { contentItem.Id = value; }
                // else if (SerializationHelpers.ContainsKey(line, "@NAME ", out value)) { contentItem.Name = value; }
            }
            catch (Exception ex)
            {
                // Don't throw.
                Console.WriteLine(ex);
            }
        }

        return audioSettings;
    }

    public static ShortcutSettings ToShortcutSettings(string[] lines)
    {
        ShortcutSettings shortcutSettings = new();
        
        foreach (string line in lines)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith('#')) continue;

                string value;
                // if (SerializationHelpers.ContainsKey(line, "@ID ",   out value)) { contentItem.Id = value; }
                // else if (SerializationHelpers.ContainsKey(line, "@NAME ", out value)) { contentItem.Name = value; }
            }
            catch (Exception ex)
            {
                // Don't throw.
                Console.WriteLine(ex);
            }
        }

        return shortcutSettings;
    }
}