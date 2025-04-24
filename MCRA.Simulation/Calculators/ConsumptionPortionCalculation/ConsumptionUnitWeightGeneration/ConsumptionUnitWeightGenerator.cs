using System.Collections.Concurrent;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation.ConsumptionUnitWeightGeneration {
    public sealed class ConsumptionUnitWeightGenerator {

        private ConcurrentDictionary<FoodConsumptionQuantification, ConsumptionUnitWeightModel> _models = new();

        /// <summary>
        /// Generates a consumption amount based on the Amount consumed, and the unit weight (if present).
        /// If uncertainty is being modelled, the amounts and/or unitweights are drawn from a lognormal distribution.
        /// </summary>
        /// <param name="consumption">The consumption of a modelled food</param>
        /// <param name="random">A Random number generator for drawing from the distribution (if uncerntainty is being modelled)</param>
        /// <returns></returns>
        public double GenerateUnitWeight(ConsumptionsByModelledFood consumption, IRandom random) {
            var unitWeightModel = GetOrCreateModel(consumption.FoodConsumption.FoodConsumptionQuantification);
            if (unitWeightModel != null) {
                return unitWeightModel.GenerateUnitWeight(random);
            } else {
                return 1;
            }
        }

        public ConsumptionUnitWeightModel GetOrCreateModel(FoodConsumptionQuantification consumptionUnit) {
            if (consumptionUnit != null) {
                if (!_models.ContainsKey(consumptionUnit)) {
                    _models.TryAdd(consumptionUnit, CreateUnitWeightModelAndCalculateParameters(consumptionUnit));
                }
                return _models[consumptionUnit];
            } else {
                return null;
            }
        }

        public void ModelUncertainty(IRandom random) {
            foreach (var model in _models.Values) {
                model.StartModellingUncertainty(random);
            }
        }

        public void StopModellingUncertainty() {
            foreach (var model in _models.Values) {
                model.StopModelingUncertainty();
            }
        }

        private ConsumptionUnitWeightModel CreateUnitWeightModelAndCalculateParameters(FoodConsumptionQuantification consumptionUnit) {
            var model = new LogNormalConsumptionUnitWeightModel();
            model.CalculateParameters(consumptionUnit);
            return model;
        }
    }
}
