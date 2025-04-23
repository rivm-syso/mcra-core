using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.IndividualExposures;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock nondietary individual day intakes
    /// </summary>
    public static class FakeNonDietaryIndividualDayIntakeGenerator {

        /// <summary>
        /// Generate non-dietary individual day exposures.
        /// </summary>
        public static List<ExternalIndividualDayExposure> Generate(
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
                     var exposuresPerSubstance = new List<IIntakePerCompound>();
                     foreach (var substance in substances) {
                         if (random.NextDouble() > fractionZeros) {
                             foreach (var route in routes) {
                                 exposuresPerSubstance.Add(new ExposurePerSubstance() {
                                     Compound = substance,
                                     Amount = random.NextDouble() * 10,
                                 });
                             }
                         }
                     }
                     var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>>();
                     exposuresPerPath[new(ExposureSource.Undefined, ExposureRoute.Oral)] = exposuresPerSubstance;
                     return g
                         .Select(r => new ExternalIndividualDayExposure(exposuresPerPath) {
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
        public static List<ExternalIndividualDayExposure> Generate(
            ICollection<SimulatedIndividual> individuals,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            double fractionZeros,
            IRandom random
        ) {
            var externalIndividualDayExposures = new List<ExternalIndividualDayExposure>();
            var count = 0;
            for (int i = 0; i < individuals.Count; i++) {
                var individual = individuals.ElementAt(i);
                var exposuresPerSubstance = new List<IIntakePerCompound>();
                foreach (var substance in substances) {
                    if (random.NextDouble() > fractionZeros) {
                        foreach (var route in routes) {
                            exposuresPerSubstance.Add(new ExposurePerSubstance() {
                                Compound = substance,
                                Amount = random.NextDouble() * 10,
                            });
                        }
                    }
                }
                var exposuresPerPath = new Dictionary<ExposurePath, List<IIntakePerCompound>> {
                    { new ExposurePath(ExposureSource.Undefined, ExposureRoute.Oral), exposuresPerSubstance }
                };
                for (int j = 0; j < individual.NumberOfDaysInSurvey; j++) {
                    var individualDayIntake = new ExternalIndividualDayExposure(exposuresPerPath) {
                        SimulatedIndividual = individual,
                        SimulatedIndividualDayId = count++,
                        Day = j.ToString(),
                    };
                    externalIndividualDayExposures.Add(individualDayIntake);
                }
            }
            return externalIndividualDayExposures;
        }
    }
}
