using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    /// <summary>
    /// Contains all information for a single individual-day.
    /// </summary>
    public sealed class NonDietaryIndividualDayIntake(
        Dictionary<ExposurePath, List<IIntakePerCompound>> exposuresPerPath
    ) : ExternalIndividualDayExposure(exposuresPerPath) {

        /// <summary>
        /// Non-dietary exposures specified per substance.
        /// </summary>
        public NonDietaryIntake NonDietaryIntake { get; set; }
        
        /// <summary>
        /// Computes the total nondietary (compound)exposures per unit body weight on this individual-day 
        /// on the external scale
        /// </summary>
        public double ExternalTotalNonDietaryIntakePerMassUnit(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return ExternalTotalNonDietaryIntake(relativePotencyFactors, membershipProbabilities)
                / (isPerPerson ? 1 : SimulatedIndividual.BodyWeight);
        }
        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day on the external scale.
        /// </summary>
        /// <returns></returns>
        public double ExternalTotalNonDietaryIntake(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return NonDietaryIntake?.ExternalTotalNonDietaryIntake(relativePotencyFactors, membershipProbabilities) ?? 0;
        }
    }
}
