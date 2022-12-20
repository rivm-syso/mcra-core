using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.TdsReductionFactorsCalculation {
    public class TdsReductionFactorsCalculator {

        private readonly IDictionary<(Food, Compound), ConcentrationDistribution> _concentrationDistributions
            = new Dictionary<(Food, Compound), ConcentrationDistribution>();

        public TdsReductionFactorsCalculator(IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions) {
            _concentrationDistributions = concentrationDistributions;
        }

        /// <summary>
        /// Returns reduction factor: based on ratio limit/percentile.
        /// </summary>
        /// <param name="foodConversionResult"></param>
        /// <param name="substance"></param>
        /// <param name="reductionFactors"></param>
        /// <returns></returns>
        public static double GetReductionFactor(
            FoodConversionResult foodConversionResult,
            Compound substance,
            IDictionary<(Food, Compound), double> reductionFactors
        ) {
            if (foodConversionResult == null) {
                return 1D;
            }
            var reductionFactor = 1D;
            foreach (var step in foodConversionResult.ConversionStepResults) {
                var fac = reductionFactors
                    .Where(r => r.Key.Item1.Code.Equals(step.FoodCodeFrom, StringComparison.OrdinalIgnoreCase) && r.Key.Item2 == substance)
                    .Select(r => r.Value);
                reductionFactor *= (fac.Any() ? fac.First() : 1D);
            }
            return reductionFactor;
        }

        /// <summary>
        /// Computes the reduction factors for the given food/substance combination.
        /// </summary>
        /// <param name="scenarioAnalysisFoods"></param>
        /// <returns></returns>
        public IDictionary<(Food, Compound), double> CalculateReductionFactors(ICollection<Food> scenarioAnalysisFoods) {
            var tdsReductionFactors = new Dictionary<(Food, Compound), double>();
            foreach (var record in _concentrationDistributions) {
                var factor = 1D;
                if (scenarioAnalysisFoods != null && !scenarioAnalysisFoods.Contains(record.Key.Item1)) {
                    factor = 1;
                } else {
                    if (record.Value != null) {
                        if (record.Value.Limit != null && record.Value.Percentile != null) {
                            factor = (double)record.Value.Limit / (double)record.Value.Percentile;
                        }
                    }
                    factor = factor > 1 ? 1F : factor;
                }
                tdsReductionFactors[record.Key] = factor;
            }
            return tdsReductionFactors;
        }
    }
}
