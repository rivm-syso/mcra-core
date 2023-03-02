using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.ISUFCalculator;
using MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation;
using MCRA.Simulation.Calculators.PopulationGeneration;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Simulation.Calculators.UnitVariabilityCalculation;

namespace MCRA.Simulation.Actions.Risks {

    public sealed class RisksModuleSettings {

        private readonly ProjectDto _project;

        public RisksModuleSettings(ProjectDto project) {
            _project = project;
        }

        public bool IsCumulative {
            get {
                return _project.AssessmentSettings.MultipleSubstances
                    && _project.AssessmentSettings.Cumulative;
            }
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public bool IsPerPerson {
            get {
                return _project.SubsetSettings.IsPerPerson;
            }
        }

        public bool CalculateRisksByFood {
            get {
                return _project.EffectModelSettings.CalculateRisksByFood;
            }
        }

        public TargetLevelType TargetDoseLevelType {
            get {
                return _project.EffectSettings.TargetDoseLevelType;
            }
        }

        public InternalConcentrationType InternalConcentrationType {
            get {
                return _project.AssessmentSettings.InternalConcentrationType;
            }
        }
    }
}
