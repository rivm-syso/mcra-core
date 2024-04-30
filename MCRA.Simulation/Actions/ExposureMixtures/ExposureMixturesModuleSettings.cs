using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation;

namespace MCRA.Simulation.Actions.ExposureMixtures {
    public sealed class ExposureMixturesModuleSettings :
        IDriverSubstanceCalculatorSettings,
        INmfCalculatorSettings {

        private readonly ExposureMixturesModuleConfig _configuration;

        public ExposureMixturesModuleSettings(ExposureMixturesModuleConfig config) {
            _configuration = config;
        }

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

        public int NumberOfIterations {
            get {
                return _configuration.MixtureSelectionIterations;
            }
        }

        public int NumberOfComponents {
            get {
                return _configuration.NumberOfMixtures;
            }
        }

        public double Sparseness {
            get {
                return _configuration.MixtureSelectionSparsenessConstraint;
            }
        }

        public double Epsilon {
            get {
                return _configuration.MixtureSelectionConvergenceCriterium;
            }
        }

        public TargetLevelType TargetDoseLevel {
            get {
                return _configuration.TargetDoseLevelType;
            }
        }

        public ExposureApproachType ExposureApproachType {
            get {
                return _configuration.ExposureApproachType;
            }
        }

        public ExposureCalculationMethod ExposureCalculationMethod {
            get {
                return _configuration.ExposureCalculationMethod;
            }
        }

        public int NumberOfClusters {
            get {
                return _configuration.NumberOfClusters;
            }
        }

        public bool AutomaticallyDeterminationOfClusters {
            get {
                return _configuration.AutomaticallyDeterminationOfClusters;
            }
        }

        public ClusterMethodType ClusterMethodType {
            get {
                return _configuration.ClusterMethodType;
            }
        }

        public NetworkAnalysisType NetworkAnalysisType {
            get {
                return _configuration.NetworkAnalysisType;
            }
        }

        public bool IsLogTransform {
            get {
                return _configuration.IsLogTransform;
            }
        }
    }
}
