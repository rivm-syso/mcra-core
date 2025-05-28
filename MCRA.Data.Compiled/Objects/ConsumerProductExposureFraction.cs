using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductExposureFraction {
        public ConsumerProduct Product { get; set; }
        public Compound Substance { get; set; }
        public ExposureRoute Route { get; set; }
        public double ExposureFraction { get; set; }
    }
}
