using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueRisksCalculation {
    public interface IIndividualSingleValueRisksCalculatorSettings {
        HealthEffectType HealthEffectType { get; }
        RiskMetricType RiskMetricType { get; }
        double Percentage { get; }
        bool UseInverseDistribution { get; }
    }
}
