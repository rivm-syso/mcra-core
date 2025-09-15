using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Actions.HbmSingleValueExposures {
    public class HbmSingleValueExposuresActionResult : IActionResult {
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
