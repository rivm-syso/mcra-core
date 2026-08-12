using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ConsumerProductExposureEstimates {
    public class ConsumerProductExposureEstimatesOutputData : IModuleOutputData {
        public IDictionary<string, ConsumerProductExposureAssessment> ConsumerProductExposureAssessments { get; set; }
        public IModuleOutputData Copy() {
            return new ConsumerProductExposureEstimatesOutputData() {
                ConsumerProductExposureAssessments = ConsumerProductExposureAssessments
            };
        }
    }
}

