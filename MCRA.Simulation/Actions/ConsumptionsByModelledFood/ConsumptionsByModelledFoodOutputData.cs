
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumptionsByModelledFood {
    public class ConsumptionsByModelledFoodOutputData : IModuleOutputData {
        public ICollection<Individual> ModelledFoodConsumers { get; set; }
        public ICollection<IndividualDay> ModelledFoodConsumerDays { get; set; }
        public ICollection<MCRA.Data.Compiled.Wrappers.ConsumptionsByModelledFood> ConsumptionsByModelledFood { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumptionsByModelledFoodOutputData() {
                ModelledFoodConsumers = ModelledFoodConsumers,
                ModelledFoodConsumerDays = ModelledFoodConsumerDays,
                ConsumptionsByModelledFood = ConsumptionsByModelledFood,
            };
        }
    }
}

