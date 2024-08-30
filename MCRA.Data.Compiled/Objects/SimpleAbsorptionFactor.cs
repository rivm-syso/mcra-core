using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SimpleAbsorptionFactor {
        public Compound Substance { get; set; }
        public double AbsorptionFactor { get; set; }
        public ExposurePathType ExposureRoute { get; set; }
    }
}
