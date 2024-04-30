using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public sealed class IndividualSingleValueRisksCalculatorSettings : IIndividualSingleValueRisksCalculatorSettings {

        private readonly SingleValueRisksModuleConfig _configuration;
        public IndividualSingleValueRisksCalculatorSettings(SingleValueRisksModuleConfig config) {
            _configuration = config;
        }

        public HealthEffectType HealthEffectType => _configuration.HealthEffectType;

        public RiskMetricType RiskMetricType => _configuration.RiskMetricType;

        public double Percentage => _configuration.Percentage;

        public bool UseInverseDistribution => _configuration.IsInverseDistribution;
    }
}
