using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public abstract class PopulationGeneratorBase {
        public abstract List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<Individual> individuals,
            ICollection<IndividualDay> individualDays,
            IRandom individualsRandomGenerator
        );
    }
}
