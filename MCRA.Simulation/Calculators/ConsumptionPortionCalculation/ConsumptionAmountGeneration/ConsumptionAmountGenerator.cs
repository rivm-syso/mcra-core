using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumptionAmountGeneration {
    public sealed class ConsumptionAmountGenerator {

        private ConcurrentDictionary<Food, ConsumptionAmountModel> _models = new();

        public double GenerateAmountFactor(ConsumptionsByModelledFood consumption, IRandom random) {
            if (consumption.FoodConsumption.FoodConsumptionQuantification != null) {
                var model = GetOrCreateModel(consumption.FoodAsMeasured, consumption.FoodConsumption.FoodConsumptionQuantification);
                return model.DrawConsumptionAmountFactor(random);
            } else {
                return 1;
            }
        }

        private ConsumptionAmountModel GetOrCreateModel(Food foodAsMeasured, FoodConsumptionQuantification consumptionUnit) {
            if (!_models.ContainsKey(foodAsMeasured)) {
                _models.TryAdd(foodAsMeasured, CreateModelAndCalculateParameters(consumptionUnit));
            }
            return _models[foodAsMeasured];
        }

        private ConsumptionAmountModel CreateModelAndCalculateParameters(FoodConsumptionQuantification consumptionUnit) {
            var model = new LogNormalConsumptionAmountModel();
            model.CalculateParameters(consumptionUnit);
            return model;
        }
    }
}
