
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation;

namespace MCRA.Simulation.Actions.SingleValueDietaryExposures {
    public class SingleValueDietaryExposuresOutputData : IModuleOutputData {
        public TargetUnit SingleValueDietaryExposureUnit { get; set; }
        public ICollection<ISingleValueDietaryExposure> SingleValueDietaryExposureResults { get; set; }
        public IModuleOutputData Copy() {
            return new SingleValueDietaryExposuresOutputData() {
                SingleValueDietaryExposureUnit = SingleValueDietaryExposureUnit,
                SingleValueDietaryExposureResults = SingleValueDietaryExposureResults
            };
        }
    }
}

