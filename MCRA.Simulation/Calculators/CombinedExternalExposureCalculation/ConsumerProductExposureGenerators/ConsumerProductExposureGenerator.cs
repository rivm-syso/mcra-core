using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.ConsumerProductExposureGenerators {
    public abstract class ConsumerProductExposureGenerator {

        /// <summary>
        /// Generates consumer product individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<ConsumerProductIndividualExposure> cpIndividualIntakes,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var cpIndividualExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => generate(
                    [.. individualExposures],
                    cpIndividualIntakes,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))))
                .ToList();

            // Check if success
            if (cpIndividualIntakes.Count == 0) {
                throw new Exception("Failed to match any consumer product exposure.");
            }
            var cpExposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = substanceAmountUnit,
                ExposureSource = ExposureSource.ConsumerProducts,
                ExternalIndividualDayExposures = cpIndividualExposures
            };
            return cpExposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<ConsumerProductIndividualExposure> cpIndividualExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        protected static ExternalIndividualDayExposure createExternalIndividualDayExposure(
            IIndividualDay individualDay,
            ConsumerProductIndividualExposure individualExposure
        ) {
            return new ExternalIndividualDayExposure(individualExposure.ExposuresPerPath) {
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
