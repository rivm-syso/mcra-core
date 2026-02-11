using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.AirConcentrationDistributions {
    public class AirConcentrationDistributionsActionResult : IActionResult {
        public IDictionary<Compound, ConcentrationModel> IndoorAirConcentrationModels { get; set; }
        public IDictionary<Compound, ConcentrationModel> OutdoorAirConcentrationModels { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
