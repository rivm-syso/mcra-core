using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Actions.OccurrencePatterns {
    public class OccurrencePatternsActionResult : IActionResult {
        public ICollection<MarginalOccurrencePattern> OccurrencePatterns { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
