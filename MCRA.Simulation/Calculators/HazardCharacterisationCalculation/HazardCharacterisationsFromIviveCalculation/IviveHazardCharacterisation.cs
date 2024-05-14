using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation {

    /// <summary>
    /// A hazard characterisation derived from IVIVE.
    /// </summary>
    public sealed class IviveHazardCharacterisation : HazardCharacterisationModel {

        /// <summary>
        /// The internal hazard characterisation.
        /// </summary>
        public double InternalHazardDose { get; set; } = double.NaN;

        /// <summary>
        /// The target unit of the test-system/dose-response model.
        /// </summary>
        public TargetUnit InternalTargetUnit { get; set; }

        /// <summary>
        /// Correction factor for translating (mol-based) dose-amounts (of dose unit of dose 
        /// response model) to (mass-based) dose-amounts aligned with external exposure.
        /// </summary>
        public double SubstanceAmountCorrectionFactor { get; set; } = 1;

        /// <summary>
        /// The internal hazard characterisation.
        /// </summary>
        public double KineticConversionFactor { get; set; } = double.NaN;

        /// <summary>
        /// The intra-species conversion factor of substance used for this IVIVE hazard characterisation.
        /// </summary>
        public double NominalIntraSpeciesConversionFactor { get; set; } = double.NaN;

        /// <summary>
        /// The interspecies conversion factor for the substance of this hazard characterisation record.
        /// </summary>
        public double NominalInterSpeciesConversionFactor { get; set; } = double.NaN;

        /// <summary>
        /// The additional conversion factor for the substance of this hazard characterisation record.
        /// </summary>
        public double AdditionalConversionFactor { get; set; } = double.NaN;

    }
}
