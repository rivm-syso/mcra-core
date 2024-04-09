using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation {
    public interface ISingleValueNonDietaryExposure {
        IDictionary<string, ExposureDeterminantCombination> SingleValueNonDietaryExposureDeterminantCombinations { get; set; }
        IList<ExposureEstimate> SingleValueNonDietaryExposureEstimates { get; set; }
        IDictionary<string, ExposureScenario> SingleValueNonDietaryExposureScenarios { get; set; }
    }
}
