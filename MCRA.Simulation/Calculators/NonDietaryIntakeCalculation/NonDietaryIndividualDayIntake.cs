using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    /// <summary>
    /// Contains all information for a single individual-day.
    /// </summary>
    public sealed class NonDietaryIndividualDayIntake {

        public NonDietaryIndividualDayIntake() { }

        /// <summary>
        /// The id assigned to the simulated individual.
        /// The sampling weight of the simulated individual.
        /// For ExposureType == acute and numberOfIterations == 0, use samplingweights to determine percentiles (USESAMPLINGWEIGHTS):
        ///   - always correct input,
        ///   - correct output;
        /// For ExposureType == acute and numberOfIterations > 0, no samplingweights to determine percentiles, weights are already in simulated exposures (DO NOT USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - output is already weighted;
        ///  For ExposureType == chronic (USESAMPLINGWEIGHTS)
        ///   - always correct input,
        ///   - correct output;
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Identifier for a simulated individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The original Individual entity.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The sampling weight of the (simulated) individual.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }

        /// <summary>
        /// The exposure day.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// Non-dietary exposures specified per substance.
        /// </summary>
        public NonDietaryIntake NonDietaryIntake { get; set; }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day of the specified route.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryExposurePerRoute(ExposurePathType exposureRoute, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            return NonDietaryIntake?.TotalNonDietaryExposurePerRoute(exposureRoute, relativePotencyFactors, membershipProbabilities) ?? 0;
        }

        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day, using the provided absorption factors.
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryIntake(IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
            return NonDietaryIntake?.TotalNonDietaryIntake(kineticConversionFactors, relativePotencyFactors, membershipProbabilities) ?? 0;
        }

        /// <summary>
        /// Computes the total nondietary (compound)exposures per unit body weight on this individual-day
        /// </summary>
        /// <returns></returns>
        public double TotalNonDietaryIntakePerMassUnit(IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors, IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            return TotalNonDietaryIntake(kineticConversionFactors, relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : this.CompartmentWeight);
        }

        /// <summary>
        /// Computes the total nondietary (compound)exposures per unit body weight on this individual-day on the external scale
        /// </summary>
        /// <returns></returns>
        public double ExternalTotalNonDietaryIntakePerMassUnit(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities, bool isPerPerson) {
            return ExternalTotalNonDietaryIntake(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : this.Individual.BodyWeight);
        }
        /// <summary>
        /// Sums all (substance) nondietary exposures on this individual-day on the external scale.
        /// </summary>
        /// <returns></returns>
        public double ExternalTotalNonDietaryIntake(IDictionary<Compound, double> relativePotencyFactors, IDictionary<Compound, double> membershipProbabilities) {
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
            var intakesPerSubstance = NonDietaryIntake.NonDietaryIntakesPerCompound
                .GroupBy(ndipc => ndipc.Compound)
                .Select(g => new AggregateIntakePerCompound() {
                    Amount = g.Sum(ndipc => ndipc.Amount),
                    Compound = g.Key,
                }).Cast<IIntakePerCompound>()
                .ToList();
            return intakesPerSubstance;
        }

        /// <summary>
        /// Weight of target compartment.
        /// </summary>
        public double CompartmentWeight {
            get {
                return Individual.BodyWeight * RelativeCompartmentWeight;
            }
        }
        public double RelativeCompartmentWeight { get; set; }
    }
}
