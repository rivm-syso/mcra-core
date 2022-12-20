using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

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
        public double TotalNonDietaryExposurePerRoute(ExposureRouteType exposureRoute, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            var routeExposures = NonDietaryIntakesPerCompound.Where(r => r.Route == exposureRoute);
            return routeExposures.Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]));
        }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day, using the provided absorption factors.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryIntake(TwoKeyDictionary<ExposureRouteType, Compound, double> absorptionFactors, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            var totalIntake = 0d;
            totalIntake += NonDietaryIntakesPerCompound
                .Where(r => r.Exposure > 0)
                .Sum(r => absorptionFactors[r.Route, r.Compound] * r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]));
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
                    .Where(r => r.Exposure > 0)
                    .Sum(r => r.Intake(relativePotencyFactors[r.Compound], membershipProbabilities[r.Compound]));
            }
            return _externalTotalNonDietaryIntake;
        }

        private double _externalTotalNonDietaryIntake = double.NaN;
    }
}