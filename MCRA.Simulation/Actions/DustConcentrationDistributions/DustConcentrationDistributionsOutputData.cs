
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {
    public class DustConcentrationDistributionsOutputData : IModuleOutputData {
        public IList<DustConcentrationDistribution> DustConcentrationDistributions { get; set; }
        public IModuleOutputData Copy() {
            return new DustConcentrationDistributionsOutputData() {
                DustConcentrationDistributions = DustConcentrationDistributions,
            };
        }
    }
}

