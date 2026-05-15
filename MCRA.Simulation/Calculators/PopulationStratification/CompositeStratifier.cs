using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {


    public class CompositeStratificationLevel : ICompositeStratificationLevel {

        public string Code => string.Join(", ", Parts.Select(c => c.Code));
        public string Name => string.Join(", ", Parts.Select(c => c.Name));
        public List<IStratificationLevel> Parts { get; private set; }

        public CompositeStratificationLevel(List<IStratificationLevel> parts) {
            Parts = parts;
        }
    }

    /// <summary>
    /// Stratifier for stratification based on the occupation scenario assigned
    /// to the individuals.
    /// </summary>
    public class CompositeStratifier : PopulationStratifier {

        private readonly Dictionary<string, List<IStratificationLevel>> _composite = [];
        private readonly List<PopulationStratifier> _stratifiers;

        public CompositeStratifier(
            List<PopulationStratifier> stratifiers
        ) {
            _stratifiers = stratifiers;
        }

        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        public List<IStratificationLevel> GetLevels(SimulatedIndividual individual) {
            var parts = new List<IStratificationLevel>();
            foreach (var stratifier in _stratifiers) {
                var level = stratifier.GetLevel(individual);
                if (level != null) {
                    parts.Add(level);
                }
            }
            return parts;
        }

        public override IStratificationLevel GetLevel(SimulatedIndividual individual) {
            var results = GetLevels(individual);
            var keys = string.Join(", ", results.Select(c => c.Code));

            if (_composite.TryGetValue(keys, out var parts)) {
                return new CompositeStratificationLevel(parts);
            } else {
                _composite[keys] = results;
                return new CompositeStratificationLevel(results);
            }
        }
    }
}
