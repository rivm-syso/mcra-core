using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock nondietary individual day intakes
    /// </summary>
    public static class FakeNonDietaryIndividualDayIntakeGenerator {

        /// <summary>
        /// Generate non-dietary individual day exposures.
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<NonDietaryIndividualDayIntake> Generate(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            double fractionZeros,
            IRandom random
        ) {
            var result = simulatedIndividualDays
                 .GroupBy(r => r.SimulatedIndividualId)
                 .SelectMany(g => {
                     var individual = g.First().Individual;
                     var nonDietaryIntakesPerCompound = new List<NonDietaryIntakePerCompound>();
                     foreach (var substance in substances) {
                         if (random.NextDouble() > fractionZeros) {
                             foreach (var route in exposureRoutes) {
                                 nonDietaryIntakesPerCompound.Add(new NonDietaryIntakePerCompound() {
                                     Compound = substance,
                                     Route = route,
                                     Amount = random.NextDouble() * 10,
                                 });
                             }
                         }
                     }
                     return g
                         .Select(r => new NonDietaryIndividualDayIntake() {
                             Individual = individual,
                             SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                             SimulatedIndividualId = r.SimulatedIndividualId,
                             IndividualSamplingWeight = r.IndividualSamplingWeight,
                             Day = r.Day,
                             NonDietaryIntake = new NonDietaryIntake() {
                                 NonDietaryIntakesPerCompound = nonDietaryIntakesPerCompound,
                             },
                             RelativeCompartmentWeight = 1,
                         })
                         .ToList();
                 })
                 .ToList();
            return result;
        }

        /// <summary>
        /// Generate non-dietary individual day exposures.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="substances"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<NonDietaryIndividualDayIntake> Generate(
            ICollection<Individual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            double fractionZeros,
            IRandom random
        ) {
            var individualDayIntakes = new List<NonDietaryIndividualDayIntake>();
            var count = 0;
            for (int i = 0; i < individuals.Count; i++) {
                var individual = individuals.ElementAt(i);
                var nonDietaryIntakesPerCompound = new List<NonDietaryIntakePerCompound>();
                foreach (var substance in substances) {
                    if (random.NextDouble() > fractionZeros) {
                        foreach (var route in exposureRoutes) {
                            nonDietaryIntakesPerCompound.Add(new NonDietaryIntakePerCompound() {
                                Compound = substance,
                                Route = route,
                                Amount = random.NextDouble() * 10,
                            });
                        }
                    }
                }
                for (int j = 0; j < individual.NumberOfDaysInSurvey; j++) {
                    var individualDayIntake = new NonDietaryIndividualDayIntake() {
                        Individual = individual,
                        SimulatedIndividualDayId = count++,
                        SimulatedIndividualId = i,
                        IndividualSamplingWeight = 1,
                        Day = j.ToString(),
                        NonDietaryIntake = new NonDietaryIntake() {
                            NonDietaryIntakesPerCompound = nonDietaryIntakesPerCompound,
                        },
                        RelativeCompartmentWeight = 1,
                    };
                    individualDayIntakes.Add(individualDayIntake);
                }
            }
            return individualDayIntakes;
        }
    }
}
