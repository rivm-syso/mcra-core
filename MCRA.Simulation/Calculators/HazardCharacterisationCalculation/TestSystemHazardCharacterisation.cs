using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {
    public sealed class TestSystemHazardCharacterisation {

        /// <summary>
        /// The hazard dose record related to this test-system hazard characterisation.
        /// </summary>
        public Data.Compiled.Objects.PointOfDeparture PoD { get; set; }

        /// <summary>
        /// Critical effect dose of the substance for the original system (i.e., animal
        /// or in vitro system) for which the limit dose was defined.
        /// </summary>
        public double HazardDose { get; set; }

        /// <summary>
        /// Dose unit of the hazard dose.
        /// </summary>
        public DoseUnit DoseUnit { get; set; }

        /// <summary>
        /// The effect of this hazard characterisation source.
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        /// The species.
        /// </summary>
        public string Species { get; set; }

        /// <summary>
        /// The organ of the test-system.
        /// </summary>
        public string Organ { get; set; }

        /// <summary>
        /// The expression type, e.g., "lipids", "creatinine".
        /// </summary>
        public ExpressionType ExpressionType { get; set; }

        /// <summary>
        /// The exposure route of the test-system.
        /// </summary>
        public ExposurePathType ExposureRoute { get; set; }

        /// <summary>
        /// Conversion factor to align the dose unit (i.e., substance amount and/or
        /// concentration mass unit) to that of the limit dose with that of the
        /// target dose.
        /// </summary>
        public double TargetUnitAlignmentFactor { get; set; } = 1;

        /// <summary>
        /// Conversion factor of species hazard dose to human equivalent hazard dose.
        /// </summary>
        public double InterSystemConversionFactor { get; set; } = 1;

        /// <summary>
        /// Within population conversion factor, e.g., to sensitive individual.
        /// </summary>
        public double IntraSystemConversionFactor { get; set; } = 1;

        /// <summary>
        /// The conversion factor due to kinetic-processes that needs to be accounted
        /// for when translating the system hazard dose to the hazard characterisation.
        /// </summary>
        public double KineticConversionFactor { get; set; } = 1;

        /// <summary>
        /// Conversion factor to translate test-system hazard characterisation type to Hazard characterisation type.
        /// </summary>
        public double ExpressionTypeConversionFactor { get; set; } = 1;

        /// <summary>
        /// Additional conversion factor to translate test-system hazard characterisation type to Hazard characterisation type.
        /// </summary>
        public double AdditionalConversionFactor { get; set; } = 1;

        /// <summary>
        /// The dose response relation associated with this hazard characterisation.
        /// </summary>
        public DoseResponseRelation DoseResponseRelation { get; set; }

    }
}
