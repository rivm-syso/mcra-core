using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using System;

namespace MCRA.Simulation.Calculators.ConsumptionAmountGeneration {
    public sealed class LogNormalConsumptionAmountModel : ConsumptionAmountModel {
        private double _sigma;

        public override void CalculateParameters(FoodConsumptionQuantification consumptionUnit) {
            _sigma = Math.Log((consumptionUnit.AmountUncertainty ?? 0 / 100.0).Squared() + 1).Sqrt();
        }

        public override double DrawConsumptionAmountFactor(IRandom random) {
            return Math.Exp(NormalDistribution.InvCDF(0, 1, random.NextDouble()) * _sigma);
        }
    }
}
