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
        /// The target unit (i.e., combination of exposure target and
        /// dose unit).
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The origin of the potency information of this compound.
        /// </summary>
        public PotencyOrigin PotencyOrigin { get; set; }

        /// <summary>
        /// The exposure target. Either external with a route or internal
        /// for a specified biological matrix.
        /// </summary>
        public ExposureTarget Target {
            get {
                return TargetUnit.Target;
            }
        }

        /// <summary>
        /// The dose unit of the hazard characterisation.
        /// </summary>
        public ExposureUnitTriple DoseUnit {
            get {
                return TargetUnit.ExposureUnit;
            }
        }

        /// <summary>
        /// The value of the hazard characterisation.
        /// </summary>
        public virtual double Value { get; set; } = double.NaN;

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
        /// Optional list of hazard characterisation uncertainty values.
        /// </summary>
        public ICollection<HazardCharacterisationUncertain> HazardCharacterisationsUncertains { get; set; }
        public ICollection<HCSubgroup> HCSubgroups { get; set; }

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

        /// <summary>
        /// Draws a hazard characterisation for an individual based on age, gender.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public double DrawIndividualHazardCharacterisationSubgroupDependent(double draw, double? age) {
            if (age == null) {
                DrawIndividualHazardCharacterisation(draw);
            }
            var record= HCSubgroups.Where(c => age >= c.AgeLower).Last();
            if (record == null) {
                DrawIndividualHazardCharacterisation(draw);
            }
            if (!double.IsNaN(GeometricStandardDeviation)) {
                return record.Value * Math.Exp(NormalDistribution.InvCDF(0, 1, draw) * Math.Log(GeometricStandardDeviation));
            }
            return record.Value;
        }

        /// <summary>
        /// Gets the specified percentile from the hazard characterisation variability
        /// distribution. Returns NaN if the hazard characterisation model does not
        /// specify a variability distribution.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public double GetVariabilityDistributionPercentile(double percentage) {
            if (!double.IsNaN(GeometricStandardDeviation)) {
                var result = Value
                    * Math.Exp(NormalDistribution.InvCDF(0, 1, percentage / 100D)
                    * Math.Log(GeometricStandardDeviation));
                return result;
            }
            return double.NaN;
        }

        /// <summary>
        /// Creates a shallow copy of this object.
        /// </summary>
        public HazardCharacterisationModel Clone() {
            return new HazardCharacterisationModel() {
                Code = this.Code,
                Substance = this.Substance,
                Effect = this.Effect,
                PotencyOrigin = this.PotencyOrigin,
                TargetUnit = this.TargetUnit,
                Value = this.Value,
                HazardCharacterisationType = this.HazardCharacterisationType,
                GeometricStandardDeviation = this.GeometricStandardDeviation,
                CombinedAssessmentFactor = this.CombinedAssessmentFactor,
                TestSystemHazardCharacterisation = this.TestSystemHazardCharacterisation,
                DoseResponseRelation = this.DoseResponseRelation,
                Reference = this.Reference,
                HazardCharacterisationsUncertains = this.HazardCharacterisationsUncertains,
                HCSubgroups = this.HCSubgroups,
            };
        }
    }
}
