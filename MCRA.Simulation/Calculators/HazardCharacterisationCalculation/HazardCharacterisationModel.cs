using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {
    public class HazardCharacterisationModel : IHazardCharacterisationModel {

        /// <summary>
        /// The substance to which this hazard dose applies.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The effect for which this hazard dose applies.
        /// </summary>
        public Effect Effect { get; set; }

        /// <summary>
        /// The substance to which this hazard dose applies.
        /// </summary>
        public Compound Substance { get; set; }

        /// <summary>
        /// The origin of the potency information of this compound.
        /// </summary>
        public PotencyOrigin PotencyOrigin { get; set; }

        /// <summary>
        /// The exposure target. Either external with a route or internal
        /// for a specified biological matrix.
        /// </summary>
        public ExposureTarget Target { get; set; }

        /// <summary>
        /// The exposure route of the hazard characterisation target.
        /// </summary>
        public ExposureRouteType ExposureRoute {
            get {
                return ExposureRoute;
            }
        }

        /// <summary>
        /// The target level of the hazard characterisation. I.e., internal/external.
        /// </summary>
        public TargetLevelType TargetDoseLevelType {
            get {
                return TargetDoseLevelType;
            }
        }

        /// <summary>
        /// The value of the hazard characterisation.
        /// </summary>
        public virtual double Value { get; set; } = double.NaN;

        /// <summary>
        /// The dose unit of the hazard characterisation.
        /// </summary>
        public ExposureUnitTriple DoseUnit { get; set; }
        
        /// <summary>
        /// The type of the hazard characterisation.
        /// </summary>
        public HazardCharacterisationType HazardCharacterisationType { get; set; }

        /// <summary>
        /// Geometric standard deviation of the distribution.
        /// </summary>
        public double GeometricStandardDeviation { get; set; } = double.NaN;

        /// <summary>
        /// Should be the multiplication factor for converting a dose on the
        /// test system of the point of departure to an equivalent dose for
        /// the hazard characterisation.
        /// </summary>
        public double CombinedAssessmentFactor { get; set; } = double.NaN;

        /// <summary>
        /// The test-system hazard characterisation from which this hazard characterisation was derived.
        /// was derived.
        /// </summary>
        public TestSystemHazardCharacterisation TestSystemHazardCharacterisation { get; set; }

        /// <summary>
        /// The dose response relation associated with this hazard characterisation.
        /// </summary>
        public DoseResponseRelation DoseResponseRelation { get; set; }

        /// <summary>
        /// Reference to the publication from which this hazard characterisation was obtained.
        /// </summary>
        public PublicationReference Reference { get; set; }

        /// <summary>
        /// Draws a hazard characterisation for an individual.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public double DrawIndividualHazardCharacterisation(double draw) {
            if (!double.IsNaN(GeometricStandardDeviation)) {
                return Value * Math.Exp(NormalDistribution.InvCDF(0, 1, draw) * Math.Log(GeometricStandardDeviation));
            }
            return Value;
        }
    }
}
