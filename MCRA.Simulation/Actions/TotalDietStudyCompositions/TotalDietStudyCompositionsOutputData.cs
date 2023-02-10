
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.TotalDietStudyCompositions {
    public class TotalDietStudyCompositionsOutputData : IModuleOutputData {
        public ILookup<Food, TDSFoodSampleComposition> TdsFoodCompositions { get; set; }
        public IModuleOutputData Copy() {
            return new TotalDietStudyCompositionsOutputData() {
                TdsFoodCompositions = TdsFoodCompositions,
            };
        }
    }
}

