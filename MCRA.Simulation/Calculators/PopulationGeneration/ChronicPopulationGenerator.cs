using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public class ChronicPopulationGenerator : PopulationGeneratorBase {

        public ChronicPopulationGenerator() {
        }

        public override List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<Individual> individuals, 
            ICollection<IndividualDay> individualDays, 
            IRandom individualsRandomGenerator
        ) {
            var result = new List<SimulatedIndividualDay>();
            var individualDaysLookup = individualDays
                .ToLookup(r => r.Individual);
            var individualDayIdCounter = 0;
            for (int i = 0; i < individuals.Count; i++) {
                var individual = individuals.ElementAt(i);
                foreach (var individualDay in individualDaysLookup[individual]) {
                    var record = new SimulatedIndividualDay() {
                        Individual = individual,
                        Day = individualDay.IdDay,
                        IndividualSamplingWeight = individual.SamplingWeight,
                        SimulatedIndividualId = i,
                        SimulatedIndividualDayId = individualDayIdCounter,
                    };
                    result.Add(record);
                    individualDayIdCounter++;
                }
            }
            return result;
        }
    }
}
