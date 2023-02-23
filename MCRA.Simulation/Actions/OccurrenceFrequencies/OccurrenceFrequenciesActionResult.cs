using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {
    public class OccurrenceFrequenciesActionResult : IActionResult {
        public IDictionary<(Food, Compound), OccurrenceFraction> OccurrenceFrequencies { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
