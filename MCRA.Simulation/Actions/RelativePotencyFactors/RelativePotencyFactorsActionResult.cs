using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public class RelativePotencyFactorsActionResult : IActionResult {
        public Compound ReferenceCompound { get; set; }
        public IDictionary<Compound, double> CorrectedRelativePotencyFactors { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
