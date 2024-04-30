using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Actions.TargetExposures {

    public sealed class TargetExposuresModuleSettings :
        IDriverSubstanceCalculatorSettings,
        INonDietaryExposureGeneratorFactorySettings {

        private readonly TargetExposuresModuleConfig _configuration;

        public TargetExposuresModuleSettings(TargetExposuresModuleConfig config) {
            _configuration = config;
        }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public bool Aggregate {
            get {
                return _configuration.Aggregate;
            }
        }

        // Non-dietary

        public bool MatchSpecificIndividuals {
            get {
                return _configuration.MatchSpecificIndividuals;
            }
        }

        public bool IsCorrelationBetweenIndividuals {
            get {
                return _configuration.IsCorrelationBetweenIndividuals;
            }
        }
        // Mixtures

        public double TotalExposureCutOff {
            get {
                return _configuration.MixtureSelectionTotalExposureCutOff;
            }
        }

        public double RatioCutOff {
            get {
                return _configuration.MixtureSelectionRatioCutOff;
            }
        }

        // Output settings

        public bool FirstModelThenAdd {
            get {
                return _configuration.FirstModelThenAdd;
            }
        }

        public IntakeModelType IntakeModelType {
            get {
                return _configuration.IntakeModelType;
            }
        }
    }
}
