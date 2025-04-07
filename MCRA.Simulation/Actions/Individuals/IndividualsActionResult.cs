using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.Individuals {
    public class IndividualsActionResult : IActionResult {
        public ICollection<IIndividualDay> Individuals { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
