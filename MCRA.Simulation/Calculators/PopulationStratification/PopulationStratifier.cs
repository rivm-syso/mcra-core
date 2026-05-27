using MCRA.General;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {

    /// <summary>
    /// Composite stratifier assigning individuals of a population in
    /// distinct categories based on the internal simple stratifier(s).
    /// </summary>
    public class PopulationStratifier : IPopulationStratifier {

        private readonly List<ISimplePopulationStratifier> _stratifiers;

        public PopulationStratifier(List<ISimplePopulationStratifier> stratifiers) {
            _stratifiers = stratifiers;
        }

        public List<OutputStratificationVariable> GetStratificationVariables() {
            return [.. _stratifiers.Select(r => r.StratificationVariable)];
        }

        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        private List<IStratificationLevel> getLevels(SimulatedIndividual individual) {
            var parts = new List<IStratificationLevel>();
            foreach (var stratifier in _stratifiers) {
                var level = stratifier?.GetLevel(individual);
                if (level != null) {
                    parts.Add(level);
                }
            }
            return parts;
        }

        public IStratificationLevel GetLevel(SimulatedIndividual individual) {
            var results = getLevels(individual);
            return new CompositeStratificationLevel(results);
        }
    }
}
