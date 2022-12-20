using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.Populations {
    public class PopulationsActionResult : IPopulationsActionResult {
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
