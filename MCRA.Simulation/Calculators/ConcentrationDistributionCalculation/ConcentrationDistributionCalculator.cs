using MCRA.Data.Compiled.Objects;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.ConcentrationDistributionCalculation {
    public sealed class ConcentrationDistributionCalculator {

        private readonly IDictionary<(Food, Compound), ConcentrationDistribution> _concentrationDistributions
            = new Dictionary<(Food, Compound), ConcentrationDistribution>();

        public ConcentrationDistributionCalculator(IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions) {
            _concentrationDistributions = concentrationDistributions;
        }

        /// <summary>
        /// Pooled standard deviation: sd = sqrt(ln(cv**2 + 1))
        /// </summary>
        /// <param name="compositions"></param>
        /// <param name="compound"></param>
        /// <returns></returns>
        public double GetStandardDeviation(List<TDSFoodSampleComposition> compositions, Compound compound) {
            var ss = 0D;
            var denominator = 0D;
            if (_concentrationDistributions != null) {
                foreach (var composition in compositions) {
                    if (_concentrationDistributions.TryGetValue((composition.Food, compound), out var concentrationDistribution)) {
                        var sd2 = 0D;
                        if (concentrationDistribution != null) {
                            var CV = concentrationDistribution.CV ?? 0;
                            sd2 = Math.Log(Math.Pow(CV, 2) + 1);
                        }
                        ss += sd2 * composition.PooledAmount;
                        denominator += composition.PooledAmount;
                    }
                }
            }
            var sigma = Math.Sqrt(ss / denominator);
            return double.IsNaN(sigma) ? 0 : sigma;
        }
    }
}
