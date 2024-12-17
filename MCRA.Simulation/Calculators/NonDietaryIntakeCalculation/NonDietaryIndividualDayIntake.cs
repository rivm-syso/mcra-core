using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    /// <summary>
    /// Contains all information for a single individual-day.
    /// </summary>
    public sealed class NonDietaryIndividualDayIntake : ExternalIndividualDayExposureBase {

        public NonDietaryIndividualDayIntake() { }

        /// <summary>
        /// Non-dietary exposures specified per substance.
        /// </summary>
        public NonDietaryIntake NonDietaryIntake { get; set; }

        /// <summary>
        /// Exposures per route/compound.
        /// </summary>
        public override Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance =>
            NonDietaryIntake.NonDietaryIntakesPerCompound
                .GroupBy(r => r.Route.GetExposurePath())
                .ToDictionary(
                    item => item.Key,
                    item => item.Cast<IIntakePerCompound>().ToList() as ICollection<IIntakePerCompound>
                );

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day of the specified route.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryExposurePerRoute(
            ExposureRoute exposureRoute,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships
        ) {
            return NonDietaryIntake?.TotalNonDietaryExposurePerRoute(exposureRoute, rpfs, memberships) ?? 0;
        }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day, using the provided absorption factors.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryIntake(
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return NonDietaryIntake?.TotalNonDietaryIntake(
                kineticConversionFactors,
                relativePotencyFactors,
                membershipProbabilities
            ) ?? 0;
        }

        /// <summary>
        /// Computes the total nondietary (compound)exposures per unit body weight on this individual-day
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryIntakePerMassUnit(
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return TotalNonDietaryIntake(
                kineticConversionFactors,
                relativePotencyFactors,
                membershipProbabilities
            ) / (isPerPerson ? 1 : Individual.BodyWeight);
        }

        /// <summary>
        /// Computes the total nondietary (compound)exposures per unit body weight on this individual-day on the external scale
        /// </summary>
        /// <returns></returns>
        public double ExternalTotalNonDietaryIntakePerMassUnit(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            return ExternalTotalNonDietaryIntake(relativePotencyFactors, membershipProbabilities) 
                / (isPerPerson ? 1 : this.Individual.BodyWeight);
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

        public ICollection<NonDietaryIntakePerCompound> GetTotalIntakesPerRouteSubstance() {
            var intakesPerRouteSubstance = NonDietaryIntake.NonDietaryIntakesPerCompound
                .GroupBy(ipc => (ipc.Compound, ipc.Route))
                .Select(g => new NonDietaryIntakePerCompound {
                    Compound = g.Key.Compound,
                    Route = g.Key.Route,
                    Amount = g.Sum(c => c.Amount),
                })
                .ToList();
            return intakesPerRouteSubstance;
        }

        public ICollection<IIntakePerCompound> GetTotalIntakesPerCompound() {
            return GetTotalExposurePerCompound();
        }
    }
}
