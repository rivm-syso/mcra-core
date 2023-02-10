
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.SubstanceAuthorisations {
    public class SubstanceAuthorisationsOutputData : IModuleOutputData {
        public IDictionary<(Food Food, Compound Substance), SubstanceAuthorisation> SubstanceAuthorisations { get; set; }
        public IModuleOutputData Copy() {
            return new SubstanceAuthorisationsOutputData() {
                SubstanceAuthorisations = SubstanceAuthorisations
            };
        }
    }
}

