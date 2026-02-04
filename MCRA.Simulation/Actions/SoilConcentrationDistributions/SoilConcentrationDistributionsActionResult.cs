using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {
    public class SoilConcentrationDistributionsActionResult : IActionResult {
        public IDictionary<Compound, ConcentrationModel> SoilConcentrationModels { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
