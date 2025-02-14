using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SimpleAbsorptionFactor {
        public Compound Substance { get; set; }
        public double AbsorptionFactor { get; set; }
        public ExposurePathType ExposurePathType { get; set; }

        public ExposureRoute ExposureRoute {
            get {
                return ExposurePathType.GetExposureRoute();
            }
        }
    }
}
