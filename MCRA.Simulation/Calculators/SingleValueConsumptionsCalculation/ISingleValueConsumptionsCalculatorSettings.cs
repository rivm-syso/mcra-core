using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueConsumptionsCalculation {
    public interface ISingleValueConsumptionsCalculatorSettings {
        bool UseSamplingWeights { get; }
        bool IsConsumersOnly { get; }
        ExposureType ExposureType { get; }
        bool IsProcessing { get; }
        bool UseBodyWeightStandardisedConsumptionDistribution { get; }
    }
}
