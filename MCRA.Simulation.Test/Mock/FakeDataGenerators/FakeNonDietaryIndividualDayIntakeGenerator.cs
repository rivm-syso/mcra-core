using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Objects;
using System.Linq;
using CommandLine;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock nondietary individual day intakes
    /// </summary>
    public static class FakeNonDietaryIndividualDayIntakeGenerator {

        /// <summary>
        /// Generate non-dietary individual day exposures.
        /// </summary>
        public static List<NonDietaryIndividualDayIntake> Generate(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            double fractionZeros,
            IRandom random
        ) {
            var result = simulatedIndividualDays
                 .GroupBy(r => r.SimulatedIndividual)
                 .SelectMany(g => {
                     var individual = g.Key;
                     var nonDietaryIntakesPerCompound = new List<IIntakePerCompound>();
                     foreach (var substance in substances) {
                         if (random.NextDouble() > fractionZeros) {
                             foreach (var route in routes) {
                                 nonDietaryIntakesPerCompound.Add(new NonDietaryIntakePerCompound() {
                                     Compound = substance,
                                     Route = route,
                                     Amount = random.NextDouble() * 10,
                                 });
                             }
                         }
                     }
                     var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                        { new ExposurePath(ExposureSource.Undefined, ExposureRoute.Oral), nonDietaryIntakesPerCompound }
                        };
                     return g
                         .Select(r => new NonDietaryIndividualDayIntake(exposuresPerPath) {
                             SimulatedIndividual = individual,
                             SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                             Day = r.Day,
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
        /// <param name="routes"></param>
        /// <param name="fractionZeros"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<NonDietaryIndividualDayIntake> Generate(
            ICollection<SimulatedIndividual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            double fractionZeros,
            IRandom random
        ) {
            var individualDayIntakes = new List<NonDietaryIndividualDayIntake>();
            var count = 0;
            for (int i = 0; i < individuals.Count; i++) {
                var individual = individuals.ElementAt(i);
                var nonDietaryIntakesPerCompound = new List<IIntakePerCompound>();
                foreach (var substance in substances) {
                    if (random.NextDouble() > fractionZeros) {
                        foreach (var route in routes) {
                            nonDietaryIntakesPerCompound.Add(new NonDietaryIntakePerCompound() {
                                Compound = substance,
                                Route = route,
                                Amount = random.NextDouble() * 10,
                            });
                        }
                    }
                }
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                    { new ExposurePath(ExposureSource.Undefined, ExposureRoute.Oral), nonDietaryIntakesPerCompound }
                };
                for (int j = 0; j < individual.NumberOfDaysInSurvey; j++) {
                    var individualDayIntake = new NonDietaryIndividualDayIntake(exposuresPerPath) {
                        SimulatedIndividual = individual,
                        SimulatedIndividualDayId = count++,
                        Day = j.ToString(),
                        NonDietaryIntake = new NonDietaryIntake() {
                            NonDietaryIntakesPerCompound = nonDietaryIntakesPerCompound
                                .Cast<NonDietaryIntakePerCompound>()
                                .ToList()
                        }
                    };
                    individualDayIntakes.Add(individualDayIntake);
                }
            }
            return individualDayIntakes;
        }
    }
}
