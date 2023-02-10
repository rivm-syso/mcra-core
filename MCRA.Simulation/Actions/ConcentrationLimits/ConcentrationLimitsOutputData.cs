
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConcentrationLimits {
    public class ConcentrationLimitsOutputData : IModuleOutputData {
        public IDictionary<(Food Food, Compound Substance), ConcentrationLimit> MaximumConcentrationLimits { get; set; }
        public IModuleOutputData Copy() {
            return new ConcentrationLimitsOutputData() {
                MaximumConcentrationLimits = MaximumConcentrationLimits,
            };
        }
    }
}

