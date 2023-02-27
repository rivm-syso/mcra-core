
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Consumptions {
    public class ConsumptionsOutputData : IModuleOutputData {
        public ICollection<Individual> ConsumerIndividuals { get; set; }
        public ICollection<IndividualDay> ConsumerIndividualDays { get; set; }
        public ICollection<FoodConsumption> SelectedFoodConsumptions { get; set; }
        public ICollection<Food> FoodsAsEaten { get; set; }
        public IndividualProperty Cofactor { get; set; }
        public IndividualProperty Covariable { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumptionsOutputData() {
                ConsumerIndividuals = ConsumerIndividuals,
                ConsumerIndividualDays = ConsumerIndividualDays,
                SelectedFoodConsumptions = SelectedFoodConsumptions,
                FoodsAsEaten = FoodsAsEaten,
                Cofactor = Cofactor,
                Covariable = Covariable
            };
        }
    }
}

