using MCRA.General;

namespace MCRA.Simulation.Calculators.AdjustmentFactorCalculation {
    public interface IAdjustmentFactorModelFactorySettings {
        AdjustmentFactorDistributionMethod AdjustmentFactorDistributionMethod { get; }
        double ParameterA { get; }
        double ParameterB { get; }
        double ParameterC { get; }
        double ParameterD { get; }
    }
}
