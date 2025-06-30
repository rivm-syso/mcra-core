using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrationDistributions {
    public class ConsumerProductConcentrationDistributionsActionResult : IActionResult {

        public IDictionary<(ConsumerProduct, Compound), ConcentrationModel> ConsumerProductConcentrationModels { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
