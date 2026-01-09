using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposureCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Objects.IndividualExposures;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.OccupationalExposureGenerators {
    public class OccupationalExposureGenerator {

        /// <summary>
        /// Generates dust individual day exposures.
        /// </summary>
        public ExternalExposureCollection Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<Compound> substances,
            ICollection<OccupationalScenario> occupationalScenarios,
            ICollection<OccupationalScenarioExposure> occupationalScenarioExposures,
            SubstanceAmountUnit substanceAmountUnit,
            ExposureType exposureType,
            int seed
        ) {
            var occupationalScenarioExposuresLookup = occupationalScenarioExposures
                .ToLookup(r => r.Scenario);
            var individualExposures = individualDays
                .AsParallel()
                .GroupBy(r => r.SimulatedIndividual.Id)
                .SelectMany(individualExposures => Generate(
                    [.. individualExposures],
                    occupationalScenarios,
                    occupationalScenarioExposuresLookup,
                    substances,
                    substanceAmountUnit,
                    exposureType,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, individualExposures.Key))
                ))
                .ToList();

            var exposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = substanceAmountUnit,
                ExposureSource = ExposureSource.Occupational,
                ExternalIndividualDayExposures = individualExposures
            };
            return exposureCollection;
        }

        protected List<IExternalIndividualDayExposure> Generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<OccupationalScenario> occupationalScenarios,
            ILookup<OccupationalScenario, OccupationalScenarioExposure> occupationalScenarioExposures,
            ICollection<Compound> substances,
            SubstanceAmountUnit substanceAmountUnit,
            ExposureType exposureType,
            IRandom random
        ) {
            if (exposureType == ExposureType.Acute) {
                throw new NotImplementedException();
            }
            var result = new List<IExternalIndividualDayExposure>();
            var individualScenarios = individualDays
                .GroupBy(r => r.SimulatedIndividual)
                .ToDictionary(
                    r => r.Key,
                    r => occupationalScenarios.ElementAt(random.Next(occupationalScenarios.Count))
                );
            foreach (var individualDay in individualDays) {
                var scenario = individualScenarios[individualDay.SimulatedIndividual];
                var exposures = occupationalScenarioExposures[scenario];
                var exposuresPerPath = occupationalScenarioExposures[scenario]
                    .GroupBy(r => r.Route)
                    .ToDictionary(
                        r => new ExposurePath(ExposureSource.Occupational, r.Key),
                        r => r
                            .GroupBy(e => e.Substance)
                            .Select(g => {
                                var totalAmount = g.Sum(e => {
                                    var amountUnitAlignmentFactor = e.Unit.SubstanceAmountUnit
                                        .GetMultiplicationFactor(substanceAmountUnit);
                                    return amountUnitAlignmentFactor * e.Value;
                                });
                                var ipc = new ExposurePerSubstance() {
                                    Compound = g.Key,
                                    Amount = totalAmount
                                };
                                return ipc;
                            })
                            .Cast<IIntakePerCompound>()
                            .ToList()
                    );
                var record = new ExternalIndividualDayExposure(exposuresPerPath) {
                    SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDay.SimulatedIndividual,
                    Day = individualDay.Day,
                };
                result.Add(record);
            }
            return result;
        }
    }
}
