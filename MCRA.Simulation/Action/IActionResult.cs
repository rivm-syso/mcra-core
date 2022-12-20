using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Action {
    public interface IActionResult {
        IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
