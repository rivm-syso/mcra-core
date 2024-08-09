using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {
    public interface IHazardCharacterisationModel {

        /// <summary>
        /// The substance to which this hazard dose applies.
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// The effect for which this hazard dose applies.
        /// </summary>
        Effect Effect { get; set; }

        /// <summary>
        /// The substance to which this hazard dose applies.
        /// </summary>
        Compound Substance { get; set; }

        /// <summary>
        /// The target unit (i.e., combination of exposure target and
        /// dose unit).
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The exposure target. Either external with a route or internal
        /// for a specified biological matrix.
        /// </summary>
        ExposureTarget Target { get; }

        /// <summary>
        /// The dose unit of the hazard characterisation.
        /// </summary>
        ExposureUnitTriple DoseUnit { get; }

        /// <summary>
        /// The type of the hazard characterisation.
        /// </summary>
        HazardCharacterisationType HazardCharacterisationType { get; set; }

        /// <summary>
        /// The value of the hazard characterisation.
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Geometric standard deviation of the distribution.
        /// </summary>
        double GeometricStandardDeviation { get; set; }

        /// <summary>
        /// The origin of the potency information of this hazard characterisation.
        /// </summary>
        PotencyOrigin PotencyOrigin { get; }

        /// <summary>
        /// Should be the multiplication factor for converting a dose on the
        /// test system of the point of departure to an equivalent dose for
        /// the hazard characterisation.
        /// </summary>
        double CombinedAssessmentFactor { get; set; }

        /// <summary>
        /// Drilldown information of the test-system from which this hazard characterisation
        /// was derived.
        /// </summary>
        TestSystemHazardCharacterisation TestSystemHazardCharacterisation { get; set; }

        /// <summary>
        /// Reference to the publication from which this hazard characterisation was obtained.
        /// </summary>
        PublicationReference Reference { get; set; }

        /// <summary>
        /// Optional list of hazard characterisation uncertainty values.
        /// </summary>
        ICollection<HazardCharacterisationUncertain> HazardCharacterisationsUncertains { get; set; }

        /// <summary>
        /// Hazard characterisations for specific sub-groups.
        /// </summary>
        ICollection<HCSubgroup> HCSubgroups { get; set; }

        /// <summary>
        /// Draws a hazard characterisation for an individual.
        /// </summary>
        /// <param name="draw"></param>
        /// <returns></returns>
        double DrawIndividualHazardCharacterisation(double draw);

        /// <summary>
        /// Draw an individual hazard characterisation for the specified subgroup.
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="age"></param>
        /// <returns></returns>
        double DrawIndividualHazardCharacterisationSubgroupDependent(double draw, double? age);

        /// <summary>
        /// Gets the variability distribution percentile for the specified percentage.
        /// Returns NaN when the model does not include variability.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        double GetVariabilityDistributionPercentile(double percentage);

        /// <summary>
        /// Creates a clone of this object.
        /// </summary>
        /// <returns></returns>
        HazardCharacterisationModel Clone();

    }
}
