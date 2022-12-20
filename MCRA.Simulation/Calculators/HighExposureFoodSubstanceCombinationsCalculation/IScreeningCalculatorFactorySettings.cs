using MCRA.General;

namespace MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation {
    public interface IScreeningCalculatorFactorySettings {
        double CriticalExposurePercentage { get; }
        double CumulativeSelectionPercentage { get; }
        double ImportanceLor { get; }
        ExposureType ExposureType { get; }
    }
}
