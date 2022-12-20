using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MCRA.Simulation.OutputGeneration {

    [XmlType(TypeName = "IndividualExposureDistributionsByCategory")]
    public sealed class UsualIntakeDistributionPerFoodAsMeasuredSection : UsualIntakeDistributionPerCategorySectionBase {

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> individualDayIntakes,
            ICollection<Compound> substances,
            ICollection<Food> foodsAsMeasured,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var observedIndividualMeansByFoodAsMeasured = computeCategorizedIndividualExposures(
                individualDayIntakes,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                isPerPerson
            );

            Categories = foodsAsMeasured
                .Select(r => new Category() {
                    Id = r.Code,
                    Name = r.Name
                })
                .ToList();
            IndividualExposuresByCategory = observedIndividualMeansByFoodAsMeasured
                .ToList();
        }

        /// <summary>
        /// Computes detailed OIMs, with exposure amounts per category.
        /// </summary>
        /// <param name="individualDayIntakes"></param>
        /// <returns></returns>
        public ICollection<CategorizedIndividualExposure> computeCategorizedIndividualExposures(
            ICollection<DietaryIndividualDayIntake> individualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            bool isPerPerson
        ) {
            var rpfs = relativePotencyFactors ?? substances?.ToDictionary(r => r, r => 1D);
            var memberships = membershipProbabilities ?? substances?.ToDictionary(r => r, r => 1D);
            var detailedObservedIndividualMeansFoodAsMeasured = new List<CategorizedIndividualExposure>();
            var result = individualDayIntakes
                .GroupBy(r => r.SimulatedIndividualId)
                .Select(g => {
                    var daysCount = g.Count();
                    var detailedObservedIndividualMean = new CategorizedIndividualExposure() {
                        SimulatedIndividualId = g.Key,
                        SamplingWeight = g.First().IndividualSamplingWeight,
                        CategoryExposures = g
                            .SelectMany(idi => idi.IntakesPerFood)
                            .GroupBy(ipf => ipf.FoodAsMeasured)
                            .Select(r => new CategoryExposure() {
                                IdCategory = r.Key.Name,
                                Exposure = r.Sum(ipf => ipf.IntakePerMassUnit(rpfs, memberships, isPerPerson)) / daysCount
                            })
                            .Where(r => r.Exposure > 0)
                            .ToList()
                    };
                    return detailedObservedIndividualMean;
                })
                .ToList();
            return result;
        }
    }
}
