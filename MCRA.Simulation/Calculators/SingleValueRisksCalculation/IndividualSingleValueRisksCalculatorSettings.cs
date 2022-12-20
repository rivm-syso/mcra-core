using MCRA.General;
using MCRA.General.Action.Settings.Dto;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public sealed class IndividualSingleValueRisksCalculatorSettings : IIndividualSingleValueRisksCalculatorSettings {

        private readonly EffectModelSettingsDto _effectModelSettings;
        public IndividualSingleValueRisksCalculatorSettings(EffectModelSettingsDto effectModelSettings) {
            _effectModelSettings = effectModelSettings;
        }
        
        public HealthEffectType HealthEffectType => _effectModelSettings.HealthEffectType;

        public RiskMetricType RiskMetricType => _effectModelSettings.RiskMetricType;

        public double Percentage => _effectModelSettings.Percentage;

        public bool UseInverseDistribution => _effectModelSettings.IsInverseDistribution;
    }
}
