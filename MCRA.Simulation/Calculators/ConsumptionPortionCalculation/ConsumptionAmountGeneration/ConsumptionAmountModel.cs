using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ConsumptionAmountGeneration {
    public abstract class ConsumptionAmountModel {
        public abstract void CalculateParameters(FoodConsumptionQuantification consumptionUnit);
        public abstract double DrawConsumptionAmountFactor(IRandom random);
    }
}
