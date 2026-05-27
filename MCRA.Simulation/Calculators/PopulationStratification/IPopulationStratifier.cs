using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {
    public interface IPopulationStratifier {
        IStratificationLevel GetLevel(SimulatedIndividual individual);
    }
}