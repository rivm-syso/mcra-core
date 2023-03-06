using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.TdsReductionFactorsCalculation {
    public class TdsReductionFactorsCalculator {

        private readonly IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> _concentrationDistributions;

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
            IDictionary<(Food Food, Compound Substance), double> reductionFactors
        ) {
            if (foodConversionResult == null) {
                return 1D;
            }
            var reductionFactor = 1D;
            //create new dictionary with suitable key (food code, substance)
            var reductionFactorLookup = reductionFactors.ToDictionary(k => (k.Key.Food.Code, k.Key.Substance), k => k.Value);
            foreach (var step in foodConversionResult.ConversionStepResults) {
                if(reductionFactorLookup.TryGetValue((step.FoodCodeFrom, substance), out var factor)) {
                    reductionFactor *= factor;
                }
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
                if (scenarioAnalysisFoods != null && !scenarioAnalysisFoods.Contains(record.Key.Food)) {
                    factor = 1;
                } else {
                    if (record.Value != null) {
                        if (record.Value.Limit.HasValue && record.Value.Percentile.HasValue) {
                            factor = record.Value.Limit.Value / record.Value.Percentile.Value;
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
