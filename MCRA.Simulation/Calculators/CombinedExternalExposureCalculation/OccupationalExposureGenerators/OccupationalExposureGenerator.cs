using System;
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

            var individuals = individualDays
                .GroupBy(r => r.SimulatedIndividual)
                .Select(r => r.Key);

            // Assign random scenarios to individuals. For acute, we assume that we have distinct simulated
            // individuals for all individual days, for chronic, we assume that we have the same simulated
            // individuals across the individual days, so we assign the scenario at the level of the simulated
            // individual.
            var random = new McraRandomGenerator(seed);
            foreach (var individual in individuals) {
                individual.OccupationalScenario = occupationalScenarios.ElementAt(random.Next(occupationalScenarios.Count));
            }

            var individualExposures = individualDays
                .AsParallel()
                .Select(r => generatedIndividualDayExposure(
                    r,
                    occupationalScenarios,
                    occupationalScenarioExposuresLookup,
                    substances,
                    substanceAmountUnit,
                    exposureType,
                    new McraRandomGenerator(RandomUtils.CreateSeed(seed, r.SimulatedIndividualDayId))
                ))
                .ToList();
            var exposureCollection = new ExternalExposureCollection {
                SubstanceAmountUnit = substanceAmountUnit,
                ExposureSource = ExposureSource.Occupational,
                ExternalIndividualDayExposures = individualExposures
            };
            return exposureCollection;
        }

        private IExternalIndividualDayExposure generatedIndividualDayExposure(
            IIndividualDay individualDay,
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
            var scenario = individualDay.SimulatedIndividual.OccupationalScenario;
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
            var result = new ExternalOccupationalIndividualDayExposure(exposuresPerPath) {
                SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                SimulatedIndividual = individualDay.SimulatedIndividual,
                Day = individualDay.Day,
                OccupationalScenario = scenario
            };
            return result;
        }
    }
}
