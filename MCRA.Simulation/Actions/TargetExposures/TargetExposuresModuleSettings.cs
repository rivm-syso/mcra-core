using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Actions.TargetExposures {

    public sealed class TargetExposuresModuleSettings :
        IDriverSubstanceCalculatorSettings,
        INonDietaryExposureGeneratorFactorySettings {

        private readonly ProjectDto _project;

        public TargetExposuresModuleSettings(ProjectDto project) {
            _project = project;
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public bool Aggregate {
            get {
                return _project.AssessmentSettings.Aggregate;
            }
        }

        // Non-dietary

        public bool MatchSpecificIndividuals {
            get {
                return _project.NonDietarySettings.MatchSpecificIndividuals;
            }
        }

        public bool IsCorrelationBetweenIndividuals {
            get {
                return _project.NonDietarySettings.IsCorrelationBetweenIndividuals;
            }
        }
        // Mixtures

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

        // Output settings

        public bool FirstModelThenAdd {
            get {
                return _project.IntakeModelSettings.FirstModelThenAdd;
            }
        }

        public IntakeModelType IntakeModelType {
            get {
                return _project.IntakeModelSettings.IntakeModelType;
            }
        }
    }
}
