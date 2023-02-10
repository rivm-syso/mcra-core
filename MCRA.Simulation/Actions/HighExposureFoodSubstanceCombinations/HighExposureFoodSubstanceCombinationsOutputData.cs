using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.HighExposureFoodSubstanceCombinations {
    public class HighExposureFoodSubstanceCombinationsOutputData : IModuleOutputData {
        public ScreeningResult ScreeningResult { get; set; }
        public IModuleOutputData Copy() {
            return new HighExposureFoodSubstanceCombinationsOutputData() {
                ScreeningResult = ScreeningResult,
            };
        }
    }
}

