using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.Stratification {

    public class OccupationalScenarioCaseControlStratificationLevel : IStratificationLevel {

        public OccupationalScenario Scenario { get; private set; }
        public CaseControlType CaseControl { get; private set; }
        public string Code => Scenario.Code;
        public string Name => $"{Scenario.Name} {CaseControl.GetDisplayName()}";

        public OccupationalScenarioCaseControlStratificationLevel(
            OccupationalScenario scenario,
            ICollection<string> controlScenarioCodes
        ) {
            CaseControl = controlScenarioCodes.Contains(scenario.Code)
                ? CaseControlType.Control
                : CaseControlType.Case;
            Scenario = scenario;
        }
    }

    /// <summary>
    /// Stratifier for stratification based on the occupation scenario assigned
    /// to the individuals.
    /// </summary>
    public class OccupationalScenarioCaseControlStratifier : PopulationStratifier {

        private readonly Dictionary<OccupationalScenario, IStratificationLevel> _stratificationLevels;

        public OccupationalScenarioCaseControlStratifier(
            IEnumerable<OccupationalScenario> scenarios,
            ICollection<string> controlScenarioCodes
        ) {
            if (controlScenarioCodes == null || !controlScenarioCodes.Any()) {
                throw new Exception("No control scenario records are specified. Supply at least one control record for a case control analysis.");
            }
            _stratificationLevels = CreateLevels(scenarios, controlScenarioCodes);
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
        private Dictionary<OccupationalScenario, IStratificationLevel> CreateLevels(
            IEnumerable<OccupationalScenario> scenarios,
            ICollection<string> controlScenarioCodes
        ) {
            return scenarios.ToDictionary(
                    r => r,
                    r => new OccupationalScenarioCaseControlStratificationLevel(r, controlScenarioCodes) as IStratificationLevel
                );
        }
    }
}