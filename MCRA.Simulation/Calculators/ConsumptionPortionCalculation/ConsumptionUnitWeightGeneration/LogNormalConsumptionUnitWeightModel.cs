using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation.ConsumptionUnitWeightGeneration {
    public sealed class LogNormalConsumptionUnitWeightModel : ConsumptionUnitWeightModel {

        private double _muUnitWeight;
        private double _sigmaUnitWeight;
        private double? _muUnitWeightWithUncertainty;

        public override void CalculateParameters(FoodConsumptionQuantification consumptionUnit) {
            if (consumptionUnit != null) {
                _sigmaUnitWeight = Math.Log((consumptionUnit.UnitWeightUncertainty ?? 0 / 100.0).Squared() + 1).Sqrt();
                _muUnitWeight = consumptionUnit.UnitWeight;
            }
        }

        public override double GenerateUnitWeight(IRandom random) {
            return _muUnitWeightWithUncertainty ?? _muUnitWeight;
        }

        public override void StartModellingUncertainty(IRandom random) {
            IsModellingUncertainty = true;
            _muUnitWeightWithUncertainty = Math.Exp(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * _sigmaUnitWeight + _muUnitWeight - 0.5 * _sigmaUnitWeight.Squared());
        }

        public override void StopModelingUncertainty() {
            IsModellingUncertainty = false;
            _muUnitWeightWithUncertainty = null;
        }
    }
}
