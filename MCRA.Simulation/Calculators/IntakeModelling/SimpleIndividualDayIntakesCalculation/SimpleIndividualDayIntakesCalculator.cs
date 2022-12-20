using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntakeModelling.IndividualAmountCalculation {

    /// <summary>
    /// Class for computing simple individual day intakes for the provided
    /// detailed individual day intakes objects.
    /// </summary>
    public class SimpleIndividualDayIntakesCalculator {

        private readonly ICollection<Compound> _substances;
        private readonly IDictionary<Compound, double> _relativePotencyFactors;
        private readonly IDictionary<Compound, double> _membershipProbabilities;
        private readonly ICollection<Food> _foodsAsMeasuredCategory;
        private readonly bool _isPerPerson;

        public SimpleIndividualDayIntakesCalculator(
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson,
            ICollection<Food> foodsAsMeasuredCategory
        ) {
            _substances = substances;
            _relativePotencyFactors = relativePotencyFactors;
            _membershipProbabilities = membershipProbabilities;
            _isPerPerson = isPerPerson;
            _foodsAsMeasuredCategory = foodsAsMeasuredCategory;
        }

        /// <summary>
        /// Computes simple individual day amounts from the given individual day intakes.
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        /// <returns></returns>
        public ICollection<SimpleIndividualDayIntake> Compute(
            ICollection<DietaryIndividualDayIntake> individualDayIntakes
        ) {
            var rpfs = _relativePotencyFactors ?? _substances?.ToDictionary(r => r, r => 1D);
            var memberships = _membershipProbabilities ?? _substances?.ToDictionary(r => r, r => 1D);
            if (_foodsAsMeasuredCategory != null) {
                var result = individualDayIntakes
                    .Select(r => new SimpleIndividualDayIntake() {
                        SimulatedIndividualId = r.SimulatedIndividualId,
                        SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                        IndividualSamplingWeight = r.IndividualSamplingWeight,
                        Amount = r.GetTotalDietaryIntakePerMassUnitPerCategory(
                            _foodsAsMeasuredCategory,
                            rpfs,
                            memberships,
                            _isPerPerson
                        ),
                        Individual = r.Individual,
                        Day = r.Day,
                    })
                    .ToList();
                return result;
            } else {
                var result = individualDayIntakes
                    .Select(r => new SimpleIndividualDayIntake() {
                        SimulatedIndividualId = r.SimulatedIndividualId,
                        SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                        IndividualSamplingWeight = r.IndividualSamplingWeight,
                        Amount = r.TotalExposurePerMassUnit(rpfs, memberships, _isPerPerson),
                        Individual = r.Individual,
                        Day = r.Day,
                    })
                    .ToList();
                return result;
            }
        }

        /// <summary>
        /// Computes simple individual intakes from the individual day intakes
        /// by grouping by simulated individual id.
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        /// <returns></returns>
        public static List<SimpleIndividualIntake> ComputeIndividualAmounts(
            ICollection<SimpleIndividualDayIntake> individualDayIntakes
        ) {
            var result = individualDayIntakes
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => {
                    var intakes = g
                        .Select(idi => idi.Amount)
                        .ToArray();
                    var numPositiveIntakeDays = intakes
                        .Where(r => r > 0)
                        .Count();
                    var totalIntake = intakes.Sum();
                    var individual = g.First().Individual;
                    return new SimpleIndividualIntake() {
                        SimulatedIndividualId = g.Key,
                        Cofactor = individual.Cofactor,
                        Covariable = individual.Covariable,
                        NumberOfDays = g.Count(),
                        NumberOfPositiveIntakeDays = numPositiveIntakeDays,
                        Intake = totalIntake / numPositiveIntakeDays,
                        DayIntakes = intakes,
                        IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                    };
                })
                .ToList();
            return result;
        }

        public List<SimpleIndividualIntake> ComputeIndividualAmounts(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            var rpfs = _relativePotencyFactors ?? _substances?.ToDictionary(r => r, r => 1D);
            var memberships = _membershipProbabilities ?? _substances?.ToDictionary(r => r, r => 1D);
            if (_foodsAsMeasuredCategory != null) {
                var result = dietaryIndividualDayIntakes
                    .GroupBy(idi => idi.SimulatedIndividualId)
                    .Select(g => {
                        var individualIntakes = g
                            .Select(idi => idi.GetTotalDietaryIntakePerMassUnitPerCategory(
                                _foodsAsMeasuredCategory,
                                rpfs,
                                memberships,
                                _isPerPerson)
                            )
                            .ToArray();
                        var numPositiveIntakeDays = individualIntakes.Where(r => r > 0).Count();
                        var totalIntake = individualIntakes.Sum();
                        var individual = g.First().Individual;
                        return new SimpleIndividualIntake() {
                            SimulatedIndividualId = g.Key,
                            Cofactor = individual.Cofactor,
                            Covariable = individual.Covariable,
                            NumberOfDays = g.Count(),
                            NumberOfPositiveIntakeDays = numPositiveIntakeDays,
                            Intake = totalIntake / numPositiveIntakeDays,
                            DayIntakes = individualIntakes,
                            IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                        };
                    })
                    .ToList();
                return result;
            } else {
                var result = dietaryIndividualDayIntakes
                    .GroupBy(idi => idi.SimulatedIndividualId)
                    .Select(g => {
                        var individualDayIntakes = g
                            .Select(idi => idi.TotalExposurePerMassUnit(
                                rpfs,
                                memberships,
                                _isPerPerson)
                            )
                            .ToArray();
                        var numPositiveIntakeDays = individualDayIntakes.Where(r => r > 0).Count();
                        var totalIntake = individualDayIntakes.Sum();
                        var individual = g.First().Individual;
                        return new SimpleIndividualIntake() {
                            SimulatedIndividualId = g.Key,
                            Cofactor = individual.Cofactor,
                            Covariable = individual.Covariable,
                            NumberOfDays = g.Count(),
                            NumberOfPositiveIntakeDays = numPositiveIntakeDays,
                            Intake = totalIntake / numPositiveIntakeDays,
                            DayIntakes = individualDayIntakes,
                            IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                        };
                    })
                    .ToList();
                return result;
            }
        }

        /// <summary>
        /// Computes the mean of the transformed individual day amounts.
        /// </summary>
        /// <param name="individualAmounts"></param>
        /// <param name="intakeTransformer"></param>
        /// <returns></returns>
        public static ICollection<ModelledIndividualAmount> ComputeMeanTransformedIndividualAmounts(
            ICollection<SimpleIndividualIntake> individualAmounts,
            IntakeTransformer intakeTransformer
        ) {
            var result = individualAmounts
                .Select(r => new ModelledIndividualAmount() {
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    Cofactor = r.Cofactor,
                    Covariable = r.Covariable,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
                    NumberOfPositiveIntakeDays = r.NumberOfPositiveIntakeDays,
                    // Note: we want the mean of the transformed amounts here, (NOT the transformed OIM)!!!
                    TransformedAmount = r.DayIntakes
                        .Where(da => !double.IsNaN(da) && da > 0)
                        .Sum(da => intakeTransformer.Transform(da)) / r.NumberOfPositiveIntakeDays,
                    TransformedDayAmounts = r.DayIntakes
                        .Select(da => intakeTransformer.Transform(da))
                        .ToArray()
                })
                .ToList();
            return result;
        }

        /// <summary>
        /// Calculate observed individual means.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <returns></returns>
        public List<DietaryIndividualIntake> ComputeObservedIndividualMeans(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            var individualDayAmounts = Compute(dietaryIndividualDayIntakes);
            return individualDayAmounts
               .GroupBy(idi => idi.SimulatedIndividualId)
               .Select(g => new DietaryIndividualIntake() {
                   SimulatedIndividualId = g.Key,
                   Individual = g.First().Individual,
                   IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                   DietaryIntakePerMassUnit = g.Average(idi => idi.Amount)
               })
               .ToList();
        }

    }
}
