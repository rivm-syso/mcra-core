using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Actions.TargetExposures {

    public sealed class TargetExposuresModuleSettings :
        IDriverSubstanceCalculatorSettings,
        INonDietaryExposureGeneratorFactorySettings ,
        IDustExposureGeneratorFactorySettings {

        private readonly TargetExposuresModuleConfig _configuration;

        public TargetExposuresModuleSettings(TargetExposuresModuleConfig config) {
            _configuration = config;
        }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public List<ExposureSource> ExposureSources {
            get {
                return _configuration.ExposureSources;
            }
        }

        public ExposureSource IndividualReferenceSet {
            get {
                return _configuration.IndividualReferenceSet;
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
                return _configuration.McrCalculationTotalExposureCutOff;
            }
        }

        public double RatioCutOff {
            get {
                return _configuration.McrCalculationRatioCutOff;
            }
        }
    }
}
