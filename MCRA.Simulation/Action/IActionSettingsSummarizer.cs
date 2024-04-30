
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Action {

    public interface IActionSettingsSummarizer {
        ActionSettingsSummary Summarize(bool isCompute, ProjectDto project = null);
    }
}
