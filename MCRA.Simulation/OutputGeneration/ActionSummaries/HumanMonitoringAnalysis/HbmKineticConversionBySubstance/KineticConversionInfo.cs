using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticConversionInfo {
        public ExposureTarget TargetFrom { get; set; }
        public ExposureTarget TargetTo { get; set; }
        public double ExposureTo { get; set; }
        public double ExposureFrom { get; set; }
    }
}
