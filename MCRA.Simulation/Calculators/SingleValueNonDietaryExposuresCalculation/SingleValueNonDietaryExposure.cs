using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation {

    public sealed class SingleValueNonDietaryExposure : ISingleValueNonDietaryExposure {
        public IDictionary<string, ExposureScenario> SingleValueNonDietaryExposureScenarios { get; set; }
        public IDictionary<string, ExposureDeterminantCombination> SingleValueNonDietaryExposureDeterminantCombinations { get; set; }
        public IList<ExposureEstimate> SingleValueNonDietaryExposureEstimates { get; set; }
    }
}
