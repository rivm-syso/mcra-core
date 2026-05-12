using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.Stratification {
    public class PopulationStratifierFactory {

        /// <summary>
        /// Creates a <see cref="PopulationStratifier"/> instance based on the specified stratification variable, 
        /// possibly using the provided additional data and/or selected scenarios for case control analysis.
        /// </summary>
        public static PopulationStratifier Create(
            OutputStratificationVariable stratificationVariable,
            ICollection<OccupationalScenario> scenarios,
            ICollection<string> controlScenarioCodes = null
        ) {
            PopulationStratifier result = stratificationVariable switch {
                OutputStratificationVariable.Sex => new GenderStratifier(),
                OutputStratificationVariable.OccupationalScenario => new SimpleOccupationalScenarioStratifier(scenarios),
                OutputStratificationVariable.CaseControl => new OccupationalScenarioCaseControlStratifier(scenarios, controlScenarioCodes),
                _ => throw new NotImplementedException($"Stratification variable {stratificationVariable} is not implemented."),
            };
            return result;
        }
    }
}
