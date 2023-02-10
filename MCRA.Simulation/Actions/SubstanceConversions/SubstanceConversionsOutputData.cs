
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SubstanceConversions {
    public class SubstanceConversionsOutputData : IModuleOutputData {
        public ICollection<SubstanceConversion> SubstanceConversions { get; set; }
        public IModuleOutputData Copy() {
            return new SubstanceConversionsOutputData() {
                SubstanceConversions = SubstanceConversions
            };
        }
    }
}

