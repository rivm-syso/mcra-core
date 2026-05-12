using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {

    public abstract class PopulationStratifier {
        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        public abstract IStratificationLevel GetLevel(SimulatedIndividual individual);
    }
}
