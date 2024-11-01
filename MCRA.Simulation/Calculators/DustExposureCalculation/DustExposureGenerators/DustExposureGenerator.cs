using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public abstract class DustExposureGenerator {

        protected List<DustIndividualDayExposure> _dustIndividualDayExposures;

        public virtual void Initialize(
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ExposureUnitTriple targetUnit,
            BodyWeightUnit targetBodyWeightUnit
        ) {
            _dustIndividualDayExposures = [.. dustIndividualDayExposures];
        }

        /// <summary>
        /// Generates acute non-dietary individual day exposures.
        /// </summary>
        public List<DustIndividualDayExposure> GenerateDustIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            int seed,
            CancellationToken cancelToken
        ) {
            var dustIndividualExposures = individualDays
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
