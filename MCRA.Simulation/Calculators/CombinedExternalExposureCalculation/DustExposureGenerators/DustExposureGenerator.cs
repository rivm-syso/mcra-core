using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
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
            ICollection<DustIndividualDayExposure> individualDayExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var individualExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => generate(
                    [.. individualExposures],
                    individualDayExposures,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))
                ))
                .ToList();

            // Check if success
            if (individualExposures.Count == 0) {
                throw new Exception("Failed to match any dust exposure.");
            }
            var exposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = substanceAmountUnit,
                ExposureSource = ExposureSource.Dust,
                ExternalIndividualDayExposures = individualExposures
            };
            return exposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> individualDayExposures,
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

        protected static ExternalIndividualDayExposure createEmptyExternalIndividualDayExposure(
            IIndividualDay individualDay,
            HashSet<ExposurePath> exposurePaths
        ) {
            var emptyExposuresPerPath = exposurePaths
                .Select(c => c)
                .ToDictionary(c => c, c => new List<IIntakePerCompound>());
            return new ExternalIndividualDayExposure(emptyExposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day,
            };
        }
    }
}
