using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {

    /// <summary>
    /// Minimal interface for stratification levels.
    /// </summary>
    public interface IStratificationLevel {
        string Code { get; }
        string Name { get; }
    }

    public abstract class PopulationStratifier {
        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        public abstract IStratificationLevel GetLevel(SimulatedIndividual individual);
    }
}
