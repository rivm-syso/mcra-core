using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public class SingleValueNonDietaryExposuresActionResult : IActionResult {
        public IList<ExposureEstimate> SingleValueNonDietaryExposureEstimates { get; set; }
        public IDictionary<string, ExposureScenario> SingleValueNonDietaryExposureScenarios { get; set; }
        public IDictionary<string, ExposureDeterminantCombination> SingleValueNonDietaryExposureDeterminantCombinations {  get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
