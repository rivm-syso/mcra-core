using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

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
            var individualCounter = 0;
            foreach (var idv in individuals) {
                var simulatedIdv = new SimulatedIndividual(idv, individualCounter++);
                foreach (var individualDay in individualDaysLookup[idv]) {
                    var record = new SimulatedIndividualDay(simulatedIdv) {
                        Day = individualDay.IdDay,
                        SimulatedIndividualDayId = individualDayIdCounter++
                    };
                    result.Add(record);
                }
            }
            return result;
        }
    }
}
