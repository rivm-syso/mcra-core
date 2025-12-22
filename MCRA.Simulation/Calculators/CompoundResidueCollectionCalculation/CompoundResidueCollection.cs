using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ResidueGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation {

    /// <summary>
    /// Holds residue data on a certain food for a certain compound.
    /// </summary>
    public sealed class CompoundResidueCollection : ResidueCollection {

        /// <summary>
        /// Constructor.
        /// </summary>
        public CompoundResidueCollection() {
            Positives = [];
            CensoredValuesCollection = [];
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public CompoundResidueCollection(ResidueCollection residueCollection) {
            Positives = residueCollection.Positives;
            CensoredValuesCollection = residueCollection.CensoredValuesCollection;
            ZerosCount = residueCollection.ZerosCount;
            StandardDeviation = residueCollection.StandardDeviation;
        }

        /// <summary>
        /// The food.
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// The substance
        /// </summary>
        public Compound Compound { get; set; }

        public override int GetHashCode() {
            return (Food.Code + Compound.Code).GetChecksum();
        }
    }
}
