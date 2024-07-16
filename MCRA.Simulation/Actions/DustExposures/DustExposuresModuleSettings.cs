using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.DustExposures {

    public sealed class DustExposuresModuleSettings {

        private readonly DustExposuresModuleConfig _moduleConfig;

        public DustExposuresModuleSettings(DustExposuresModuleConfig config) {
            _moduleConfig = config;
        }

        public DustExposuresIndividualGenerationMethod DustExposuresIndividualGenerationMethod {
            get {
                return _moduleConfig.DustExposuresIndividualGenerationMethod;
            }
        }

        public int NumberOfSimulatedIndividuals {
            get {
                return _moduleConfig.NumberOfSimulatedIndividuals;
            }
        }

        public List<ExposureRoute> SelectedExposureRoutes {
            get {
                return _moduleConfig.SelectedExposureRoutes;
            }
        }

        public double DustTimeExposed {
            get {
                return _moduleConfig.DustTimeExposed;
            }
        }
    }
}
