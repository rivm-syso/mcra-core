using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.SingleValueInternalExposuresCalculation;

namespace MCRA.Simulation.Actions.SingleValueNonDietaryExposures {
    public class SingleValueNonDietaryExposuresOutputData : IModuleOutputData {
        public TargetUnit ExposureUnit { get; set; }
        public ICollection<ISingleValueNonDietaryExposure> SingleValueInternalExposureResults { get; set; }
        public IModuleOutputData Copy() {
            return new SingleValueNonDietaryExposuresOutputData() {
                ExposureUnit = ExposureUnit,
                SingleValueInternalExposureResults = SingleValueInternalExposureResults
            };
        }
    }
}

