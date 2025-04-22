using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DustExposureGenerators {
    public abstract class DustExposureGenerator {

        /// <summary>
        /// Generates dust individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var dustIndividualExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => generate(
                    [.. individualExposures],
                    dustIndividualDayExposures,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))
                ))
                .ToList();

            // Check if success
            if (dustIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any dust exposure.");
            }
            var dustExposureCollection = new ExternalExposureCollection {
                ExposureUnit = new ExposureUnitTriple(substanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                ExposureSource = ExposureSource.Dust,
                ExternalIndividualDayExposures = dustIndividualExposures
            };
            return dustExposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        protected ExternalIndividualDayExposure createExternalIndividualDayExposure(
            IIndividualDay individualDay,
            DustIndividualDayExposure individualDayExposure
        ) {
            return new ExternalIndividualDayExposure(individualDayExposure.ExposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day,
            };
        }
    }
}
