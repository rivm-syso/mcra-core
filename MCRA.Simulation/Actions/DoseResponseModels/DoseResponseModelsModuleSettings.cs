using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Actions.DoseResponseModels {

    public class DoseResponseModelsModuleSettings {

        private readonly ProjectDto _project;

        public DoseResponseModelsModuleSettings(ProjectDto project) {
            _project = project;
        }

        public string CodeReferenceSubstance {
            get {
                return _project.EffectSettings?.CodeReferenceCompound;
            }
        }
    }
}
