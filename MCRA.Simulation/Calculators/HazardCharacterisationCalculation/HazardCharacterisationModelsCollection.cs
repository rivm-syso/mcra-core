using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {

    /// <summary>
    /// This class holds a collection of hazard characterisations for a combination of biological matrix 
    /// and expression type.
    /// </summary>
    public sealed class HazardCharacterisationModelsCollection {

        /// <summary>
        /// Initializes a new instance of the <see cref="HazardCharacterisationModelsCollection" /> class.
        /// </summary>
        public HazardCharacterisationModelsCollection() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazardCharacterisationModelCompoundsCollection" /> class.
        /// </summary>
        public HazardCharacterisationModelsCollection(
            ICollection<IHazardCharacterisationModel> hazardCharacterisationModels,
            TargetUnit targetUnit
        ) {
            TargetUnit = targetUnit;
            HazardCharacterisationModels = hazardCharacterisationModels;
        }

        /// <summary>
        /// The target unit, the exposure target and its unit.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }
       
        /// <summary>
        /// Definitions of hazard characterisations per substance.
        /// </summary>
        public ICollection<IHazardCharacterisationModel> HazardCharacterisationModels { get; set; }
    }
}
