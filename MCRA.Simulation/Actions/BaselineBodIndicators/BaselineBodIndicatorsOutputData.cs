
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.BaselineBodIndicators {
    public class BaselineBodIndicatorsOutputData : IModuleOutputData {
        public IList<BaselineBodIndicator> BaselineBodIndicators { get; set; }
        public IModuleOutputData Copy() {
            return new BaselineBodIndicatorsOutputData() {
                BaselineBodIndicators = BaselineBodIndicators
            };
        }
    }
}
