using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {
    public sealed class ConsumptionsByModelledFoodActionResult : IActionResult {
        public ICollection<Individual> ModelledFoodConsumers { get; set; }
        public ICollection<IndividualDay> ModelledFoodConsumerDays { get; set; }
        public List<Data.Compiled.Wrappers.ConsumptionsByModelledFood> ConsumptionsByModelledFood { get; set; }
        public ICollection<FoodConsumption> ConsumptionsFoodsAsEaten { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
