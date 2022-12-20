using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.ComponentCalculation.NmfCalculation;

namespace MCRA.Simulation.Actions.ExposureMixtures {
    public sealed class ExposureMixturesModuleSettings :
        IDriverSubstanceCalculatorSettings,
        INmfCalculatorSettings {

        private readonly ProjectDto _project;

        public ExposureMixturesModuleSettings(ProjectDto project) {
            _project = project;
        }

        public double TotalExposureCutOff {
            get {
                return _project.MixtureSelectionSettings.TotalExposureCutOff;
            }
        }

        public double RatioCutOff {
            get {
                return _project.MixtureSelectionSettings.RatioCutOff;
            }
        }

        public int NumberOfIterations {
            get {
                return _project.MixtureSelectionSettings.NumberOfIterations;
            }
        }

        public int NumberOfComponents {
            get {
                return _project.MixtureSelectionSettings.K;
            }
        }

        public double Sparseness {
            get {
                return _project.MixtureSelectionSettings.SW;
            }
        }

        public double Epsilon {
            get {
                return _project.MixtureSelectionSettings.Epsilon;
            }
        }

        public TargetLevelType TargetDoseLevel {
            get {
                return _project.EffectSettings.TargetDoseLevelType;
            }
        }

        public ExposureApproachType ExposureApproachType {
            get {
                return _project.MixtureSelectionSettings.ExposureApproachType;
            }
        }

        public InternalConcentrationType InternalConcentrationType {
            get {
                return _project.MixtureSelectionSettings.InternalConcentrationType;
            }
        }

        public int NumberOfClusters {
            get {
                return _project.MixtureSelectionSettings.NumberOfClusters;
            }
        }

        public bool AutomaticallyDeterminationOfClusters {
            get {
                return _project.MixtureSelectionSettings.AutomaticallyDeterminationOfClusters;
            }
        }

        public ClusterMethodType ClusterMethodType {
            get {
                return _project.MixtureSelectionSettings.ClusterMethodType;
            }
        }

        public NetworkAnalysisType NetworkAnalysisType {
            get {
                return _project.MixtureSelectionSettings.NetworkAnalysisType;
            }
        }
    }
}
