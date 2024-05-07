using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class AggregateIndividualDayExposure : AggregateIndividualExposure, ITargetIndividualDayExposure, IExternalIndividualDayExposure {

        public AggregateIndividualDayExposure() { }

        private double _totalConcentrationAtTarget = double.NaN;

        /// <summary>
        /// Identifier for a simulated individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// Reference/identifier of the (dietary/modelled) individual day.
        /// </summary>
        public string Day { get; set; }

        public NonDietaryIndividualDayIntake NonDietaryIndividualDayIntake { get; set; }

        public DietaryIndividualDayIntake DietaryIndividualDayIntake { get; set; }

        /// <summary>
        /// Target exposure per bodyweight
        /// </summary>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <returns></returns>
        public new double TotalConcentrationAtTarget(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson) {
            // NOTE: this method is newly implemented in this dericed class because it is using performance optimization by caching the _totalConcentrationAtTarget
            if (double.IsNaN(_totalConcentrationAtTarget)) {
                _totalConcentrationAtTarget = TotalAmountAtTarget(relativePotencyFactors, membershipProbabilities) / (isPerPerson ? 1 : RelativeCompartmentWeight * SimulatedIndividualBodyWeight);
            }
            return _totalConcentrationAtTarget;
        }

        /// <summary>
        /// Returns whether there is exposure of multiple substances.
        /// </summary>
        /// <returns></returns>
        public bool IsCoExposure() {
            var result = ExposuresPerRouteSubstance
                .SelectMany(r => r.Value.Where(e => e.Amount > 0))
                .Select(r => r.Compound)
                .Distinct();
            return result.Count() > 1;
        }

        public new AggregateIndividualDayExposure Clone() {
            return new AggregateIndividualDayExposure() {
                SimulatedIndividualId = SimulatedIndividualId,
                SimulatedIndividualDayId = SimulatedIndividualDayId,
                Individual = Individual,
                Day = Day,
                IndividualSamplingWeight = IndividualSamplingWeight,
                ExposuresPerRouteSubstance = ExposuresPerRouteSubstance,
                DietaryIndividualDayIntake = DietaryIndividualDayIntake,
                NonDietaryIndividualDayIntake = NonDietaryIndividualDayIntake,
                ExternalIndividualDayExposures = ExternalIndividualDayExposures,
                RelativeCompartmentWeight = RelativeCompartmentWeight,
            };
        }
    }
}
