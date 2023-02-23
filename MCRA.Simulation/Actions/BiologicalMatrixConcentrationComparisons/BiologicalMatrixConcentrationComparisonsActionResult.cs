using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.BiologicalMatrixConcentrationComparisons {
    public sealed class BiologicalMatrixConcentrationComparisonsActionResult : IActionResult {
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
