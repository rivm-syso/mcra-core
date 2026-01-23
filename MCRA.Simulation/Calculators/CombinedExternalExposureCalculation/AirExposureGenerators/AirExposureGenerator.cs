using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {
    public abstract class AirExposureGenerator {

        /// <summary>
        /// Generates air individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<AirIndividualExposure> airIndividualExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var airExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => generate(
                    [.. individualExposures],
                    airIndividualExposures,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))))
                .ToList();

            // Check if success
            if (airExposures.Count == 0) {
                throw new Exception("Failed to match any air exposure.");
            }
            var airExposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = substanceAmountUnit,
                ExposureSource = ExposureSource.Air,
                ExternalIndividualDayExposures = airExposures
            };
            return airExposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<AirIndividualExposure> airIndividualExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        protected static ExternalIndividualDayExposure createExternalIndividualDayExposure(
            IIndividualDay individualDay,
            AirIndividualExposure individualDayExposure
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
