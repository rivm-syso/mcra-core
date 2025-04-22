using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.General;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {
    public abstract class SoilExposureGenerator {

        /// <summary>
        /// Generates soil individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var soilIndividualExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => generate(
                    [.. individualExposures],
                    soilIndividualDayExposures,
                    substances,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))
                ))
                .ToList();

            // Check if success
            if (soilIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any soil exposure.");
            }
            var soilExposureCollection = new ExternalExposureCollection {
                ExposureUnit = new ExposureUnitTriple(substanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                ExposureSource = ExposureSource.Soil,
                ExternalIndividualDayExposures = soilIndividualExposures
            };
            return soilExposureCollection;
        }

        protected abstract List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );

        protected ExternalIndividualDayExposure createExternalIndividualDayExposure(
            IIndividualDay individualDay,
            SoilIndividualDayExposure soilIndividualDayExposure
        ) {
            return new ExternalIndividualDayExposure(soilIndividualDayExposure.ExposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day,
            };
        }
    }
}
