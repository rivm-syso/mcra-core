using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.Stratification {

    public class SimpleOccupationalScenarioStratificationLevel : IStratificationLevel {
        public OccupationalScenario Scenario { get; private set; }
        public string Code => Scenario.Code;
        public string Name => Scenario.Name;
        public SimpleOccupationalScenarioStratificationLevel(OccupationalScenario scenario) {
            Scenario = scenario;
        }
    }


    /// <summary>
    /// Stratifier for stratification based on the occupation scenario assigned
    /// to the individuals.
    /// </summary>
    public class SimpleOccupationalScenarioStratifier : PopulationStratifier {

        private readonly Dictionary<OccupationalScenario, IStratificationLevel> _stratificationLevels;

        public SimpleOccupationalScenarioStratifier(IEnumerable<OccupationalScenario> scenarios) {
            _stratificationLevels = CreateLevels(scenarios);
        }

        /// <summary>
        /// Gets the stratification level of the individual.
        /// </summary>
        public override IStratificationLevel GetLevel(SimulatedIndividual individual) {
            if (individual.OccupationalScenario != null) {
                return _stratificationLevels.TryGetValue(individual.OccupationalScenario, out var val) ? val : null;
            }
            return null;
        }

        /// <summary>
        /// Creates a stratification level lookup dictionary for the provided occupational scenarios.
        /// </summary>
        private static Dictionary<OccupationalScenario, IStratificationLevel> CreateLevels(
            IEnumerable<OccupationalScenario> scenarios
        ) {
            return scenarios.ToDictionary(
                    r => r,
                    r => new SimpleOccupationalScenarioStratificationLevel(r) as IStratificationLevel
                );

        }
    }
}
