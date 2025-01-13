
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Action {

    public interface IActionSettingsSummarizer {
        ActionSettingsSummary Summarize(ProjectDto project = null);
    }
}
