using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {
    public abstract class SoilExposureGenerator {

        /// <summary>
        /// Generates soil individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<SoilIndividualExposure> soilIndividualExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var soilExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => generate(
                    [.. individualExposures],
                    soilIndividualExposures,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))
                ))
                .ToList();

            if (soilExposures.Count == 0) {
                throw new Exception("Failed to match any soil exposure.");
            }
            var soilExposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = substanceAmountUnit,
                ExposureSource = ExposureSource.Soil,
                ExternalIndividualDayExposures = soilExposures
            };
            return soilExposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<SoilIndividualExposure> soilIndividualExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        protected ExternalIndividualDayExposure createExternalIndividualDayExposure(
            IIndividualDay individualDay,
            SoilIndividualExposure soilIndividualExposure
        ) {
            return new ExternalIndividualDayExposure(soilIndividualExposure.ExposuresPerPath) {
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
