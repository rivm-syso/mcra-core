using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.DustExposures {

    public sealed class DustExposuresModuleSettings {

        private readonly DustExposuresModuleConfig _moduleConfig;

        public DustExposuresModuleSettings(DustExposuresModuleConfig config) {
            _moduleConfig = config;
        }

        public ExposureType ExposureType {
            get {
                return _moduleConfig.ExposureType;
            }
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

        public int NumberOfSimulatedIndividualDays {
            get {
                return _moduleConfig.NumberOfSimulatedIndividualDays;
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
