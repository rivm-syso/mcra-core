using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock nondietary individual intakes
    /// </summary>
    public static class FakeNonDietaryIndividualIntakeGenerator {
        /// <summary>
        /// Generate non-dietary individual day exposures.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<NonDietaryIndividualIntake> Generate(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            double fractionZeros,
            IRandom random
        ) {
            var result = individuals
                .Select(g => {
                    var individual = g;
                    var nonDietaryIntakesPerCompound = new List<NonDietaryIntakePerCompound>();
                    foreach (var substance in substances) {
                        if (random.NextDouble() > fractionZeros) {
                            foreach (var route in exposureRoutes) {
                                nonDietaryIntakesPerCompound.Add(new NonDietaryIntakePerCompound() {
                                    Compound = substance,
                                    Route = route.GetExposureRoute(),
                                    Amount = random.NextDouble() * 10,
                                });
                            }
                        }
                    }
                    return new NonDietaryIndividualIntake() {
                        Individual = individual,
                        NumberOfDays = individual.NumberOfDaysInSurvey,
                        NonDietaryIntakePerBodyWeight = nonDietaryIntakesPerCompound.Sum(c => c.Amount) / individual.BodyWeight,
                        IndividualSamplingWeight = individual.SamplingWeight,
                        SimulatedIndividualId = individual.Id,
                        NonDietaryIndividualDayIntakes= null,
                    };
                })
                .ToList();
            return result;
        }
    }
}
