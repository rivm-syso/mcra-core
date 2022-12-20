using MCRA.General;

namespace MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation {
    public interface IIntakeCalculatorFactorySettings {
        DietaryExposuresDetailsLevel DietaryExposuresDetailsLevel { get; }
        bool IsSampleBased { get; }
        bool IsCorrelation { get; }
        bool IsSingleSamplePerDay { get; }
        int NumberOfMonteCarloIterations { get; }
        ExposureType ExposureType { get; }
        bool TotalDietStudy { get; }
        bool UseScenario { get; }
    }
}
