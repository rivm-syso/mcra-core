using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock external exposures
    /// </summary>
    public static class FakeExternalExposureGenerator {

        /// <summary>
        /// Creates external individual exposures
        /// </summary>
        public static List<IExternalIndividualExposure> CreateExternalIndividualExposures(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePath> paths,
            int seed,
            double percentageZeros = 0.0
        ) {
            McraRandomGenerator randomZeros = null;
            if (percentageZeros > 0.0) {
                randomZeros = new McraRandomGenerator(seed + 12345);
            }

            var result = simulatedIndividualDays
            .GroupBy(r => r.SimulatedIndividual)
            .Select(g => {
                var indvDayExposures = CreateExternalIndividualDayExposures(
                    [.. g],
                    substances,
                    paths,
                    seed + g.Key.Id,
                    randomZeros,
                    percentageZeros
                );
                var exposuresPerPath = indvDayExposures
                        .SelectMany(x => x.ExposuresPerPath)
                        .GroupBy(r => r.Key)
                        .ToDictionary(
                            d => d.Key,
                            d => d.SelectMany(r => r.Value).ToList()
                        );

                var exposure = new ExternalIndividualExposure(g.Key, exposuresPerPath) {
                    ExternalIndividualDayExposures = indvDayExposures
                };
                return exposure;
            })
            .Cast<IExternalIndividualExposure>()
            .ToList();
            
            return result;
        }

        /// <summary>
        /// Creates external individual day exposures
        /// </summary>
        public static List<IExternalIndividualDayExposure> CreateExternalIndividualDayExposures(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePath> paths,
            IRandom random,
            IRandom randomZeros = null,
            double percentageZeros = 0.0
        ) {
            var result = simulatedIndividualDays
                .Select(r => new ExternalIndividualDayExposure(fakeRouteIntakes(substances, paths, random, 
                        randomZeros, percentageZeros)) {
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    Day = r.Day,
                    SimulatedIndividual = r.SimulatedIndividual
                })
                .Cast<IExternalIndividualDayExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates external individual day exposures
        /// </summary>
        public static List<IExternalIndividualDayExposure> CreateExternalIndividualDayExposures(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposurePath> paths,
            int seed,
            IRandom randomZeros = null,
            double percentageZeros = 0.0
        ) {
            var random = new McraRandomGenerator(seed);
            return CreateExternalIndividualDayExposures(simulatedIndividualDays, substances, paths, random, randomZeros, percentageZeros);
        }

        /// <summary>
        /// Creates mock routes for substances.
        /// </summary>
        private static Dictionary<ExposurePath, List<IIntakePerCompound>> fakeRouteIntakes(
            ICollection<Compound> substances,
            ICollection<ExposurePath> paths,
            IRandom random,
            IRandom randomZeros = null,
            double percentageZeros = 0.0
        ) {
            double amount() {
                var amount = random.NextDouble();
                if (randomZeros != null && randomZeros.NextDouble(0.0, 100.0) < percentageZeros) {
                    amount = 0.0;
                }
                return amount;
            }
            var result = paths
                .ToDictionary(
                    r => r,
                    r => substances
                        .Select(s => new AggregateIntakePerCompound() {
                            Compound = s,
                            Amount = amount(),
                        })
                        .Cast<IIntakePerCompound>()
                        .ToList()
                    );
            return result;
        }
    }
}
