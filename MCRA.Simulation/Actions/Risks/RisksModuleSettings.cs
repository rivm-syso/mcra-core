using MCRA.General;
using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Actions.Risks {

    public sealed class RisksModuleSettings {

        private readonly ProjectDto _project;

        public RisksModuleSettings(ProjectDto project) {
            _project = project;
        }

        public bool IsMultipleSubstances {
            get {
                return _project.AssessmentSettings.MultipleSubstances;
            }
        }

        public bool IsCumulative {
            get {
                return _project.AssessmentSettings.MultipleSubstances
                    && _project.EffectModelSettings.CumulativeRisk;
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

        public RiskMetricType RiskMetricType {
            get {
                return _project.EffectModelSettings.RiskMetricType;
            }
        }

        public RiskMetricCalculationType RiskMetricCalculationType {
            get {
                return _project.EffectModelSettings.RiskMetricCalculationType;
            }
        }

        public HealthEffectType HealthEffectType {
            get {
                return _project.EffectModelSettings.HealthEffectType;
            }
        }

        public double[] RiskPercentiles {
            get {
                return RiskMetricType == RiskMetricType.MarginOfExposure
                    ? _project.OutputDetailSettings.SelectedPercentiles
                        .Select(c => 100 - c).Reverse().ToArray()
                    : _project.OutputDetailSettings.SelectedPercentiles;
            }
        }

        public bool UseInverseDistribution {
            get {
                return _project.EffectModelSettings.IsInverseDistribution;
            }
        }
    }
}
