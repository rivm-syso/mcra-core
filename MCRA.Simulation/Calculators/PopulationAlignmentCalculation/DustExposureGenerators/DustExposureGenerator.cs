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
            int seed
        ) {
            var dustIndividualExposures = individualDays
                .AsParallel()
                .WithDegreeOfParallelism(100)
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualDays => {
                    var dustIndividualDayExposure = createDustIndividualExposure(
                        individualDays,
                        dustIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDays.First().SimulatedIndividualDayId))
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

        protected abstract List<DustIndividualDayExposure> createDustIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
