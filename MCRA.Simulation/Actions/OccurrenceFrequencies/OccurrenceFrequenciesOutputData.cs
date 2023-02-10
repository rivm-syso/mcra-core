
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Actions.OccurrenceFrequencies {
    public class OccurrenceFrequenciesOutputData : IModuleOutputData {
        public IDictionary<(Food Food, Compound Substance), OccurrenceFraction> OccurrenceFractions { get; set; }
        public IModuleOutputData Copy() {
            return new OccurrenceFrequenciesOutputData() {
                OccurrenceFractions = OccurrenceFractions
            };
        }
    }
}

