using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    /// <summary>
    /// Summarizes all info for non-dietary exposures.
    /// </summary>
    public sealed class NonDietaryIntake {

        /// <summary>
        /// The non-dietary exposures per substance.
        /// </summary>
        public List<NonDietaryIntakePerCompound> NonDietaryIntakesPerCompound { get; set; }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day of the specified route.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryExposurePerRoute(
            ExposureRoute exposureRoute,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            var routeExposures = NonDietaryIntakesPerCompound.Where(r => r.Route == exposureRoute);
            return routeExposures.Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]));
        }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day, using the provided absorption factors.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryIntake(
            IDictionary<(ExposureRoute, Compound), double> kineticConversionFactors,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            var totalIntake = 0d;
            totalIntake += NonDietaryIntakesPerCompound
                .Where(r => r.Amount > 0)
                .Sum(r => kineticConversionFactors[(r.Route, r.Compound)] * r.EquivalentSubstanceAmount(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]));
            return totalIntake;
        }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day on the external scale.
        /// </summary>
        /// <returns></returns>
        public double ExternalTotalNonDietaryIntake(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            if (double.IsNaN(_externalTotalNonDietaryIntake)) {
                _externalTotalNonDietaryIntake = 0d;
                _externalTotalNonDietaryIntake += NonDietaryIntakesPerCompound
                    .Where(r => r.Amount > 0)
                    .Sum(r => r.EquivalentSubstanceAmount(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]));
            }
            return _externalTotalNonDietaryIntake;
        }

        private double _externalTotalNonDietaryIntake = double.NaN;
    }
}