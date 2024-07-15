using MCRA.General.Action.Settings;

namespace MCRA.General.Action.ActionSettingsManagement {
    public interface IActionSettingsManager {
        ActionType ActionType { get; }
        void Verify(ProjectDto project);
        void InitializeAction(ProjectDto project);
        Dictionary<SettingsTemplateType, string> GetAvailableTiers();
    }
}
