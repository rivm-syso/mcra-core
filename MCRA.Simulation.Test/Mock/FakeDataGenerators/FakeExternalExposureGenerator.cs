using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
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
            int seed
        ) {
            var result = simulatedIndividualDays
                .GroupBy(r => r.SimulatedIndividual)
                .Select(g => {
                    var indvDayExposures = CreateExternalIndividualDayExposures(g.ToList(), substances, paths, seed + g.Key.Id);
                    var exposure = new ExternalIndividualExposure(g.Key) {
                        ExternalIndividualDayExposures = indvDayExposures,
                        ExposuresPerPath = indvDayExposures
                            .SelectMany(x => x.ExposuresPerPath)
                            .GroupBy(r => r.Key)
                            .ToDictionary(
                                d => d.Key,
                                d => d.SelectMany(r => r.Value).ToList()
                            )
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
            IRandom random
        ) {
            var result = simulatedIndividualDays
                .Select(r => new ExternalIndividualDayExposure(fakeRouteIntakes(substances, paths, random)) {
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
            int seed
        ) {
            var random = new McraRandomGenerator(seed);
            return CreateExternalIndividualDayExposures(simulatedIndividualDays, substances, paths, random);
        }

        /// <summary>
        /// Creates mock routes for substances.
        /// </summary>
        private static Dictionary<ExposurePath, List<IIntakePerCompound>> fakeRouteIntakes(
            ICollection<Compound> substances,
            ICollection<ExposurePath> paths,
            IRandom random
        ) {
            var result = paths
                .ToDictionary(
                    r => r,
                    r => substances
                        .Select(s => new AggregateIntakePerCompound() {
                            Compound = s,
                            Amount = random.NextDouble(),
                        })
                        .Cast<IIntakePerCompound>()
                        .ToList()
                    );
            return result;
        }
    }
}
