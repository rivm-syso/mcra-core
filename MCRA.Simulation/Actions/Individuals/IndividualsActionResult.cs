using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Actions.Individuals {
    public class IndividualsActionResult : IActionResult {
        public ICollection<IIndividualDay> Individuals { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
