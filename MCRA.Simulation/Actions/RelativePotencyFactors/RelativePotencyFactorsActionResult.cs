using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public class RelativePotencyFactorsActionResult : IActionResult {
        public IDictionary<Compound, double> CorrectedRelativePotencyFactors { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
