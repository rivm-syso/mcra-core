using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.SoilExposureGenerators {
    public abstract class SoilExposureGenerator {

        /// <summary>
        /// Generates soil individual day exposures.
        /// </summary>
        public ExternalExposureCollection GenerateSoilIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed,
            CancellationToken cancelToken
        ) {
            var soilIndividualExposures = individualDays
                .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
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
                .Cast<IExternalIndividualDayExposure>()
                .ToList();

            // Check if success
            if (soilIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any soil exposure");
            }
            var soilExposureCollection = new ExternalExposureCollection {
                ExposureUnit = new ExposureUnitTriple(substanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                ExposureSource = ExposureSource.Soil,
                ExternalIndividualDayExposures = soilIndividualExposures
            };
            return soilExposureCollection;
        }

        protected abstract SoilIndividualDayExposure createSoilIndividualExposure(
            IIndividualDay individualDay,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
