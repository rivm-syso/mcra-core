
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConcentrationDistributions {
    public class ConcentrationDistributionsOutputData : IModuleOutputData {
        public IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> ConcentrationDistributions { get; set; }
        public IModuleOutputData Copy() {
            return new ConcentrationDistributionsOutputData() {
                ConcentrationDistributions = ConcentrationDistributions
            };
        }
    }
}

