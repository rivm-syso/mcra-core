using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {
    public class HighExposureFoodSubstanceCombinationsActionResult : IActionResult {
        public ScreeningResult ScreeningResult { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
