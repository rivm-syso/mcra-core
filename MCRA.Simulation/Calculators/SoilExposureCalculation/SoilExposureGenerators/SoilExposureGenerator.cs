using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public abstract class SoilExposureGenerator {

        /// <summary>
        /// Generates soil individual day exposures.
        /// </summary>
        public ICollection<SoilIndividualDayExposure> GenerateSoilIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            int seed,
            CancellationToken cancelToken
        ) {
            var soilIndividualExposures = individualDays
                .GroupBy(r => r.SimulatedIndividualId, (key, g) => g.First())
                .AsParallel()
                .WithCancellation(cancelToken)
                .WithDegreeOfParallelism(100)
                .Select(individualDay => {
                    var soilIndividualDayExposure = createSoilIndividualExposure(
                        individualDay,
                        soilIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDay.SimulatedIndividualDayId))
                    );
                    return soilIndividualDayExposure;
                })
                .ToList();

            // Check if success
            if (soilIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any soil exposure to a dietary exposure");
            }
            return soilIndividualExposures;
        }

        protected abstract SoilIndividualDayExposure createSoilIndividualExposure(
            IIndividualDay individualDay,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
