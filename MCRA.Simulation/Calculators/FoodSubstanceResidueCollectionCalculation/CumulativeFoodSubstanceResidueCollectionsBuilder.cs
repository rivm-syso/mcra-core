using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation {
    public class CumulativeFoodSubstanceResidueCollectionsBuilder {

        /// <summary>
        /// Initialize the cumulative potency collection for the sample-based approach
        /// </summary>
        public Dictionary<Food, FoodSubstanceResidueCollection> Create(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            Compound cumulativeCompound,
            IDictionary<Compound, double> correctedRpfs
        ) {
            var cumulativeCompoundCollections = new List<FoodSubstanceResidueCollection>();
            foreach (var sampleCompoundCollection in sampleCompoundCollections) {
                var cumulativeResidues = sampleCompoundCollection.SampleCompoundRecords
                    .Select(c => c.ImputedCumulativePotency(correctedRpfs))
                    .ToList();
                var positives = cumulativeResidues.Where(r => r > 0).ToList();
                var zeros = cumulativeResidues.Where(r => r == 0).ToList();
                var collection = new FoodSubstanceResidueCollection() {
                    Compound = cumulativeCompound,
                    Food = sampleCompoundCollection.Food,
                    CensoredValuesCollection = [],
                    Positives = positives,
                    ZerosCount = zeros.Count
                };
                cumulativeCompoundCollections.Add(collection);
            }
            var result = new Dictionary<Food, FoodSubstanceResidueCollection>();
            foreach (var record in cumulativeCompoundCollections) {
                result.Add(record.Food, record);
            }
            return result;
        }
    }
}
