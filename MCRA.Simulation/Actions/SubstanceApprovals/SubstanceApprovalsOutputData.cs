
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SubstanceApprovals {
    public class SubstanceApprovalsOutputData : IModuleOutputData {
        public IDictionary<Compound, SubstanceApproval> SubstanceApprovals { get; set; }
        public IModuleOutputData Copy() {
            return new SubstanceApprovalsOutputData() {
                SubstanceApprovals = SubstanceApprovals
            };
        }
    }
}

