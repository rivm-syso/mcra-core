using CommandLine;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

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
            if (routes.Count == 0) {
                return [];
            }
            var result = simulatedIndividualDays
                 .GroupBy(r => r.SimulatedIndividual)
                 .SelectMany(g => {
                     var individual = g.Key;
                     var nonDietaryIntakesPerCompound = new List<NonDietaryIntakePerCompound>();
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
                     var exposuresPerPath = nonDietaryIntakesPerCompound
                        .GroupBy(i => i.Route)
                        .Select(g => (
                            ExposurePath: new ExposurePath(ExposureSource.Undefined, g.Key),
                            Intakes: g.Select(e => e).Cast<IIntakePerCompound>().ToList()
                        ))
                        .ToDictionary();
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
