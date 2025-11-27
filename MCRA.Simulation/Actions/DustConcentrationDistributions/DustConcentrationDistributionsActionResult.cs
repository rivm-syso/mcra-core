using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {
    public class DustConcentrationDistributionsActionResult : IActionResult {
        public IDictionary<Compound, ConcentrationModel> DustConcentrationModels { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
