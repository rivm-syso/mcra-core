using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation {

    /// <summary>
    /// Holds residue data on a certain food for a certain compound.
    /// </summary>
    public sealed class FoodSubstanceResidueCollection : ResidueCollection {

        /// <summary>
        /// The food.
        /// </summary>
        public Food Food { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FoodSubstanceResidueCollection() {
            Positives = [];
            CensoredValuesCollection = [];
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FoodSubstanceResidueCollection(ResidueCollection residueCollection) {
            Positives = residueCollection.Positives;
            CensoredValuesCollection = residueCollection.CensoredValuesCollection;
            ZerosCount = residueCollection.ZerosCount;
            StandardDeviation = residueCollection.StandardDeviation;
        }

        public override int GetHashCode() {
            return (Food.Code + Compound.Code).GetChecksum();
        }
    }
}
