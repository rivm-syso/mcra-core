using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.FoodSpecificSubstanceConversionCalculaton {
    public static class FoodSpecificSubstanceConversionCalculator {

        public static List<SampleCompoundCollection> Compute(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            ICollection<DeterministicSubstanceConversionFactor> foodSubstanceConversionFactors
        ) {
            var result = sampleCompoundCollections.ToDictionary(r => r.Food, r => r.Clone());
            foreach (var conversionFactor in foodSubstanceConversionFactors) {
                var food = conversionFactor.Food;
                var activeSubstance = conversionFactor.ActiveSubstance;
                var measuredSubstance = conversionFactor.MeasuredSubstance;
                if (result.TryGetValue(food, out var collection)) {
                    var sampleCompounds = collection.SampleCompoundRecords
                        .Where(r => r.SampleCompounds.ContainsKey(activeSubstance))
                        .Select(r => r.SampleCompounds[activeSubstance])
                        .ToList();
                    var matchedRecords = sampleCompounds
                        .Where(r => r.MeasuredSubstance == measuredSubstance)
                        .ToList();
                    foreach (var record in matchedRecords) {
                        record.Residue = record.Residue * conversionFactor.ConversionFactor;
                    }
                }
            }
            return [.. result.Values];
        }
    }
}
