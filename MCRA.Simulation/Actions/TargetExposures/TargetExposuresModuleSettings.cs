using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;

namespace MCRA.Simulation.Actions.TargetExposures {

    public sealed class TargetExposuresModuleSettings :
        IDriverSubstanceCalculatorSettings {

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
        public bool StandardisedNormalisedUrine {
            get {
                return _configuration.StandardisedNormalisedUrine;
            }
        }
        public bool StandardisedBlood {
            get {
                return _configuration.StandardisedBlood;
            }
        }
        public ExpressionType SelectedExpressionType {
            get {
                return _configuration.SelectedExpressionType;
            }
        }
        public PopulationAlignmentMethod NonDietaryPopulationAlignmentMethod {
            get {
                return _configuration.NonDietaryPopulationAlignmentMethod;
            }
        }
        public PopulationAlignmentMethod DustPopulationAlignmentMethod {
            get {
                return _configuration.DustPopulationAlignmentMethod;
            }
        }
        public PopulationAlignmentMethod SoilPopulationAlignmentMethod {
            get {
                return _configuration.SoilPopulationAlignmentMethod;
            }
        }

    }
}
