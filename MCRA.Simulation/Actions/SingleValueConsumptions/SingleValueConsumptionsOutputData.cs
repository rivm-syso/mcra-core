
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {
    public class SingleValueConsumptionsOutputData : IModuleOutputData {
        public ICollection<SingleValueConsumptionModel> SingleValueConsumptionModels { get; set; }
        public ConsumptionIntakeUnit SingleValueConsumptionIntakeUnit { get; set; }
        public BodyWeightUnit SingleValueConsumptionBodyWeightUnit { get; set; }
        public ICollection<PopulationConsumptionSingleValue> FoodConsumptionSingleValues { get; set; }
        public IModuleOutputData Copy() {
            return new SingleValueConsumptionsOutputData() {
                SingleValueConsumptionModels = SingleValueConsumptionModels,
                FoodConsumptionSingleValues = FoodConsumptionSingleValues,
                SingleValueConsumptionBodyWeightUnit = SingleValueConsumptionBodyWeightUnit,
                SingleValueConsumptionIntakeUnit = SingleValueConsumptionIntakeUnit
            };
        }
    }
}

