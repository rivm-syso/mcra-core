using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.Risks {

    public sealed class RisksModuleSettings {

        private readonly RisksModuleConfig _configuration;

        public RisksModuleSettings(RisksModuleConfig config) {
            _configuration = config;
        }

        public bool IsMultipleSubstances {
            get {
                return _configuration.MultipleSubstances;
            }
        }

        public bool IsCumulative {
            get {
                return _configuration.MultipleSubstances
                    && _configuration.CumulativeRisk;
            }
        }

        public ExposureType ExposureType {
            get {
                return _configuration.ExposureType;
            }
        }

        public bool CalculateRisksByFood {
            get {
                return _configuration.CalculateRisksByFood;
            }
        }

        public TargetLevelType TargetDoseLevelType {
            get {
                return _configuration.TargetDoseLevelType;
            }
        }

        public ExposureCalculationMethod ExposureCalculationMethod {
            get {
                return _configuration.ExposureCalculationMethod;
            }
        }

        public RiskMetricType RiskMetricType {
            get {
                return _configuration.RiskMetricType;
            }
        }

        public RiskMetricCalculationType RiskMetricCalculationType {
            get {
                return _configuration.RiskMetricCalculationType;
            }
        }

        public HealthEffectType HealthEffectType {
            get {
                return _configuration.HealthEffectType;
            }
        }

        public double[] RiskPercentiles {
            get {
                return RiskMetricType == RiskMetricType.HazardExposureRatio
                    ? _configuration.SelectedPercentiles
                        .Select(c => 100 - c).Reverse().ToArray()
                    : _configuration.SelectedPercentiles.ToArray();
            }
        }

        public bool UseInverseDistribution {
            get {
                return _configuration.IsInverseDistribution;
            }
        }

        public bool SkipPrivacySensitiveOutputs {
            get {
                return _configuration.SkipPrivacySensitiveOutputs;
            }
        }
    }
}
