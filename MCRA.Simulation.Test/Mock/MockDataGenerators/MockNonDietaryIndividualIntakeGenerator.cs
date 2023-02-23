using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock nondietary individual intakes
    /// </summary>
    public static class MockNonDietaryIndividualIntakeGenerator {
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
            ICollection<ExposureRouteType> exposureRoutes,
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
                                    Route = route,
                                    Exposure = random.NextDouble() * 10,
                                });
                            }
                        }
                    }
                    return new NonDietaryIndividualIntake() {
                        Individual = individual,
                        NumberOfDays = individual.NumberOfDaysInSurvey,
                        NonDietaryIntakePerBodyWeight = nonDietaryIntakesPerCompound.Sum(c => c.Exposure) / individual.BodyWeight,
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
