
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DeterministicSubstanceConversionFactors {
    public class DeterministicSubstanceConversionFactorsOutputData : IModuleOutputData {
        public ICollection<DeterministicSubstanceConversionFactor> DeterministicSubstanceConversionFactors { get; set; }
        public IModuleOutputData Copy() {
            return new DeterministicSubstanceConversionFactorsOutputData() {
                DeterministicSubstanceConversionFactors = DeterministicSubstanceConversionFactors
            };
        }
    }
}

