using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.ResidueGeneration {
    public sealed class SampleBasedResidueGenerator : IResidueGenerator {

        /// <summary>
        /// The sample compound collections used for sample based residue generation
        /// </summary>
        private readonly IDictionary<Food, SampleCompoundCollection> _sampleCompoundCollections;

        public SampleBasedResidueGenerator(
            IDictionary<Food, SampleCompoundCollection> sampleCompoundCollections
        ) {
            _sampleCompoundCollections = sampleCompoundCollections;
        }

        /// <summary>
        /// Draws residues for the target food and substances using the supplied random generator
        /// and using a multicompound algorithm for drawing residues.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="substances"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<CompoundConcentration> GenerateResidues(
            Food food,
            ICollection<Compound> substances,
            IRandom random
        ) {
            var selectedSampleCompoundRecords = _sampleCompoundCollections[food].SampleCompoundRecords;
            var iRecord = random.Next(selectedSampleCompoundRecords.Count);
            var concentrations = new List<CompoundConcentration>();
            if (selectedSampleCompoundRecords.Any()) {
                var sampleCompounds = selectedSampleCompoundRecords[iRecord].SampleCompounds;
                foreach (var substance in substances) {
                    if (sampleCompounds.TryGetValue(substance, out var sampleCompound)) {
                        if (sampleCompound.IsPositiveResidue) {
                            concentrations.Add(new CompoundConcentration() {
                                Compound = substance,
                                Concentration = (float)sampleCompound.Residue,
                            });
                        }
                    }
                }
            }
            return concentrations;
        }
    }
}
