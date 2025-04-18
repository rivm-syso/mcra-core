using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DustExposureGenerators {
    public abstract class DustExposureGenerator {

        /// <summary>
        /// Generates dust individual day exposures.
        /// </summary>
        public ExternalExposureCollection GenerateDustIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            SubstanceAmountUnit substanceAmountUnit,
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
                .Cast<IExternalIndividualDayExposure>()
                .ToList();

            // Check if success
            if (dustIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any dust exposure");
            }
            var dustExposureCollection = new ExternalExposureCollection {
                ExposureUnit = new ExposureUnitTriple(substanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                ExposureSource = ExposureSource.Dust,
                ExternalIndividualDayExposures = dustIndividualExposures
            };
            return dustExposureCollection;
        }

        protected abstract DustIndividualDayExposure createDustIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
