using SaturnData.Notation.Core;
using SaturnEdit.Systems;

namespace SaturnEdit.UndoRedo;

public class BuildChartOperation : IOperation
{
    private readonly string oldReading = ChartSystem.Entry.Reading;
    private readonly string oldBpmMessage = ChartSystem.Entry.BpmMessage;
    private readonly float oldClearThreshold = ChartSystem.Entry.ClearThreshold;
    private readonly Timestamp oldChartEnd = ChartSystem.Entry.ChartEnd;
    
    public void Revert()
    {
        bool autoChartEnd = ChartSystem.Entry.AutoChartEnd;
        ChartSystem.Entry.AutoChartEnd = false;
        ChartSystem.Entry.ChartEnd = oldChartEnd;
        
        ChartSystem.Chart.Build(ChartSystem.Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);

        ChartSystem.Entry.AutoChartEnd = autoChartEnd;
        ChartSystem.Entry.Reading = oldReading;
        ChartSystem.Entry.BpmMessage = oldBpmMessage;
        ChartSystem.Entry.ClearThreshold = oldClearThreshold;
    }

    public void Apply()
    {
        ChartSystem.Chart.Build(ChartSystem.Entry, (float?)AudioSystem.AudioChannelAudio?.Length ?? 0, SettingsSystem.RenderSettings.SaturnJudgeAreas);
    }
}