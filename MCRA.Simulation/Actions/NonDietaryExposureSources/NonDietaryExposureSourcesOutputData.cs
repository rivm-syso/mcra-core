
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.NonDietaryExposureSources {
    public class NonDietaryExposureSourcesOutputData : IModuleOutputData {
        public ICollection<NonDietaryExposureSource> NonDietaryExposureSources { get; set; }
        public IModuleOutputData Copy() {
            return new NonDietaryExposureSourcesOutputData() {
                NonDietaryExposureSources = NonDietaryExposureSources
            };
        }
    }
}

