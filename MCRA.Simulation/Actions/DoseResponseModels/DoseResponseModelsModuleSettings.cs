using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.DoseResponseModels {

    public class DoseResponseModelsModuleSettings {

        private readonly DoseResponseModelsModuleConfig _configuration;

        public DoseResponseModelsModuleSettings(DoseResponseModelsModuleConfig config) {
            _configuration = config;
        }

        public string CodeReferenceSubstance {
            get {
                return _configuration.CodeReferenceSubstance;
            }
        }
    }
}
