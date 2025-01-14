using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock external exposures
    /// </summary>
    public static class FakeExternalExposureGenerator {
        /// <summary>
        /// Creates external individual exposures
        /// </summary>
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="routes"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<IExternalIndividualExposure> CreateExternalIndividualExposures(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            int seed
        ) {
            var result = simulatedIndividualDays
                .GroupBy(r => r.SimulatedIndividual)
                .Select(g => {
                    var indvDayExposures = CreateExternalIndividualDayExposures(g.ToList(), substances, routes, seed + g.Key.Id);
                    var exposure = new ExternalIndividualExposure(g.Key) {
                        ExternalIndividualDayExposures = indvDayExposures,
                        ExposuresPerRouteSubstance = indvDayExposures
                            .SelectMany(x => x.ExposuresPerRouteSubstance)
                            .GroupBy(r => r.Key)
                            .ToDictionary(
                                d => d.Key,
                                d => d.SelectMany(r => r.Value).ToList() as ICollection<IIntakePerCompound>
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
        /// <param name="simulatedIndividualDays"></param>
        /// <param name="substances"></param>
        /// <param name="routes"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<IExternalIndividualDayExposure> CreateExternalIndividualDayExposures(
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            int seed
        ) {
            var random = new McraRandomGenerator(seed);
            var result = simulatedIndividualDays
                .Select(r => new ExternalIndividualDayExposure {
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    Day = r.Day,
                    SimulatedIndividual = r.SimulatedIndividual,
                    ExternalExposuresPerPath = fakeRouteIntakes(substances, routes, random)
                })
                .Cast<IExternalIndividualDayExposure>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Creates mock routes for substances.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="routes"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static Dictionary<ExposureRoute, List<IIntakePerCompound>> fakeRouteIntakes(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            IRandom random
        ) {
            var result = routes
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
