using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.AirExposureCalculation {
    public abstract class AirExposureGenerator {

        /// <summary>
        /// Generates air individual day exposures.
        /// </summary>
        public ICollection<AirIndividualDayExposure> GenerateAirIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            int seed,
            CancellationToken cancelToken
        ) {
            var airIndividualExposures = individualDays
                .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var airIndividualDayExposure = createAirIndividualExposure(
                        individualDay,
                        airIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    return airIndividualDayExposure;
                })
                .ToList();

            // Check if success
            if (airIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any air exposure to a dietary exposure");
            }
            return airIndividualExposures;
        }

        protected abstract AirIndividualDayExposure createAirIndividualExposure(
            IIndividualDay individualDay,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
