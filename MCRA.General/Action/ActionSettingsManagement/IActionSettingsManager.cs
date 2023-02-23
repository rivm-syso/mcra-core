using MCRA.General.Action.Settings.Dto;
using MCRA.General.ModuleDefinitions;

namespace MCRA.General.Action.ActionSettingsManagement {
    public interface IActionSettingsManager {
        ActionType ActionType { get; }
        void Verify(ProjectDto project);
        void InitializeAction(ProjectDto project);
        Dictionary<string, string> GetAvailableTiers();
        void SetTier(ProjectDto project, ModuleTier tier, bool cascadeInputTiers);
        void SetTier(ProjectDto project, string idTier, bool cascadeInputTiers);
    }
}
