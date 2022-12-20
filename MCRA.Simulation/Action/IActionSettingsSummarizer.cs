using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Action {

    public interface IActionSettingsSummarizer {
        ActionSettingsSummary Summarize(ProjectDto project);
    }
}
