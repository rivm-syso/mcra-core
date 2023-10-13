using MCRA.General;
using MCRA.General.Action.Settings;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public sealed class IndividualSingleValueRisksCalculatorSettings : IIndividualSingleValueRisksCalculatorSettings {

        private readonly EffectModelSettings _effectModelSettings;
        public IndividualSingleValueRisksCalculatorSettings(EffectModelSettings effectModelSettings) {
            _effectModelSettings = effectModelSettings;
        }
        
        public HealthEffectType HealthEffectType => _effectModelSettings.HealthEffectType;

        public RiskMetricType RiskMetricType => _effectModelSettings.RiskMetricType;

        public double Percentage => _effectModelSettings.Percentage;

        public bool UseInverseDistribution => _effectModelSettings.IsInverseDistribution;
    }
}
