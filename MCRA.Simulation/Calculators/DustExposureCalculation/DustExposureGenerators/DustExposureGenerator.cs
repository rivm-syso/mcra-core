using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public abstract class DustExposureGenerator {

        /// <summary>
        /// Generates dust individual day exposures.
        /// </summary>
        public ICollection<DustIndividualDayExposure> GenerateDustIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            int seed,
            CancellationToken cancelToken
        ) {
            var dustIndividualExposures = individualDays
                .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var dustIndividualDayExposure = createDustIndividualExposure(
                        individualDay,
                        dustIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    return dustIndividualDayExposure;
                })
                .ToList();

            // Check if success
            if (dustIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any dust exposure to a dietary exposure");
            }
            return dustIndividualExposures;
        }

        protected abstract DustIndividualDayExposure createDustIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
