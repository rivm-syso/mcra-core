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
        /// The exposure route of the test-system.
        /// </summary>
        ExposureRouteType ExposureRoute { get; set; }

        /// <summary>
        /// The target level of the hazard characterisation. I.e., internal/external.
        /// </summary>
        TargetLevelType TargetDoseLevelType { get; set; }

        /// <summary>
        /// The type of the hazard characterisation.
        /// </summary>
        HazardCharacterisationType HazardCharacterisationType { get; set; }

        /// <summary>
        /// The value of the hazard characterisation.
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// The dose unit of the hazard characterisation.
        /// </summary>
        TargetUnit DoseUnit { get; set; }

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
        /// Draws a hazard characterisation for an individual.
        /// </summary>
        /// <param name="draw"></param>
        /// <returns></returns>
        double DrawIndividualHazardCharacterisation(double draw);
    }
}
