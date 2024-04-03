using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation {
    public class CumulativeCompoundResidueCollectionsBuilder {

        /// <summary>
        /// Initialize the cumulative potency collection for the sample-based approach
        /// </summary>
        public Dictionary<Food, CompoundResidueCollection> Create(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            Compound cumulativeCompound,
            IDictionary<Compound, double> correctedRpfs
        ) {
            var cumulativeCompoundCollections = new List<CompoundResidueCollection>();
            foreach (var sampleCompoundCollection in sampleCompoundCollections) {
                var cumulativeResidues = sampleCompoundCollection.SampleCompoundRecords
                    .Select(c => c.ImputedCumulativePotency(correctedRpfs))
                    .ToList();
                var positives = cumulativeResidues.Where(r => r > 0).ToList();
                var zeros = cumulativeResidues.Where(r => r == 0).ToList();
                var collection = new CompoundResidueCollection() {
                    Compound = cumulativeCompound,
                    Food = sampleCompoundCollection.Food,
                    CensoredValuesCollection = new List<CensoredValue>(),
                    Positives = positives,
                    ZerosCount = zeros.Count
                };
                cumulativeCompoundCollections.Add(collection);
            }
            var result = new Dictionary<Food, CompoundResidueCollection>();
            foreach (var record in cumulativeCompoundCollections) {
                result.Add(record.Food, record);
            }
            return result;
        }
    }
}
