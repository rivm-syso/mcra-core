using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics.RandomGenerators;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.General;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.AirExposureGenerators {
    public abstract class AirExposureGenerator {

        /// <summary>
        /// Generates air individual day exposures.
        /// </summary>
        public ExternalExposureCollection GenerateAirIndividualDayExposures(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            SubstanceAmountUnit substanceAmountUnit,
            int seed
        ) {
            var airIndividualExposures = individualDays
                .AsParallel()
                .WithDegreeOfParallelism(100)
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualDays => {
                    var airIndividualDayExposure = createAirIndividualExposure(
                        individualDays,
                        airIndividualDayExposures,
                        substances,
                        new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualDays.First().SimulatedIndividualDayId))
                    );
                    return airIndividualDayExposure;
                })
                .Cast<IExternalIndividualDayExposure>()
                .ToList();

            // Check if success
            if (airIndividualExposures.Count == 0) {
                throw new Exception("Failed to match any air exposure");
            }
            var airExposureCollection = new ExternalExposureCollection {
                ExposureUnit = new ExposureUnitTriple(substanceAmountUnit, ConcentrationMassUnit.PerUnit, TimeScaleUnit.PerDay),
                ExposureSource = ExposureSource.Air,
                ExternalIndividualDayExposures = airIndividualExposures
            };
            return airExposureCollection;
        }

        protected abstract List<AirIndividualDayExposure> createAirIndividualExposure(
            IGrouping<int, IIndividualDay> individualDays,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom randomIndividual
        );
    }
}
