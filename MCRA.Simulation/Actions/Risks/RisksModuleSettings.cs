﻿using MCRA.General;
using MCRA.General.Action.Settings;

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
                    && _project.RisksSettings.CumulativeRisk;
            }
        }

        public ExposureType ExposureType {
            get {
                return _project.AssessmentSettings.ExposureType;
            }
        }

        public bool CalculateRisksByFood {
            get {
                return _project.RisksSettings.CalculateRisksByFood;
            }
        }

        public TargetLevelType TargetDoseLevelType {
            get {
                return _project.EffectSettings.TargetDoseLevelType;
            }
        }

        public ExposureCalculationMethod ExposureCalculationMethod {
            get {
                return _project.AssessmentSettings.ExposureCalculationMethod;
            }
        }

        public RiskMetricType RiskMetricType {
            get {
                return _project.RisksSettings.RiskMetricType;
            }
        }

        public RiskMetricCalculationType RiskMetricCalculationType {
            get {
                return _project.RisksSettings.RiskMetricCalculationType;
            }
        }

        public HealthEffectType HealthEffectType {
            get {
                return _project.RisksSettings.HealthEffectType;
            }
        }

        public double[] RiskPercentiles {
            get {
                return RiskMetricType == RiskMetricType.HazardExposureRatio
                    ? _project.OutputDetailSettings.SelectedPercentiles
                        .Select(c => 100 - c).Reverse().ToArray()
                    : _project.OutputDetailSettings.SelectedPercentiles;
            }
        }

        public bool UseInverseDistribution {
            get {
                return _project.RisksSettings.IsInverseDistribution;
            }
        }
    }
}
