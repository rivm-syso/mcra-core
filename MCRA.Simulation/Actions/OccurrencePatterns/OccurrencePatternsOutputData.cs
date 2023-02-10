
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.Actions.OccurrencePatterns {
    public class OccurrencePatternsOutputData : IModuleOutputData {
        public Dictionary<Food, List<MarginalOccurrencePattern>> MarginalOccurrencePatterns { get; set; }
        public ICollection<OccurrencePattern> RawAgriculturalUses { get; set; }
        public IModuleOutputData Copy() {
            return new OccurrencePatternsOutputData() {
                MarginalOccurrencePatterns = MarginalOccurrencePatterns,
                RawAgriculturalUses = RawAgriculturalUses
            };
        }
    }
}

