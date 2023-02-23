using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;

namespace MCRA.Simulation.Calculators.FocalCommodityMeasurementReplacementCalculation {
    public sealed class FocalCommodityMeasurementRemovalCalculator : IFocalCommodityMeasurementReplacementCalculator {

        /// <summary>
        /// Generates a new sample/substance collection from the provided collection
        /// with the measurements of the focal food/substance combinations being removed.
        /// </summary>
        /// <param name="baseSampleCompoundCollections"></param>
        /// <param name="focalCombinations"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public Dictionary<Food, SampleCompoundCollection> Compute(
            IDictionary<Food, SampleCompoundCollection> baseSampleCompoundCollections,
            ICollection<(Food Food, Compound Substance)> focalCombinations,
            IRandom generator
        ) {
            var groupedFocalCombinations = focalCombinations.GroupBy(r => r.Food);

            // Clone original sample/substance collection
            var result = baseSampleCompoundCollections.Values
                .Select(r => r.Clone())
                .ToDictionary(r => r.Food);

            // Loop over groupings by food
            foreach (var focalCombinationGroup in groupedFocalCombinations) {

                // Get the food from the focal combination grouping
                var food = focalCombinationGroup.Key;

                // Get the substances from the grouping
                var substancesForReplacement = focalCombinationGroup.Select(r => r.Substance).ToList();

                // If no records for imputation are found, then continue (nothing to remove)
                if (!result.TryGetValue(food, out var baseSampleCompoundCollection)) {
                    continue;
                }

                // Remove all substance concentrations that were not yet replaced
                var counter = 0;
                while (counter < baseSampleCompoundCollection.SampleCompoundRecords.Count) {
                    var recordForImputation = baseSampleCompoundCollection.SampleCompoundRecords[counter];
                    foreach (var substance in substancesForReplacement) {
                        recordForImputation.SampleCompounds[substance] = new SampleCompound() {
                            MeasuredSubstance = substance,
                            ActiveSubstance = substance,
                            Residue = double.NaN,
                            ResType = ResType.MV,
                            Loq = double.NaN,
                            Lod = double.NaN,
                        };
                    }
                    counter++;
                }
            }

            return result;
        }
    }
}
