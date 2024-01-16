using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class KineticAbsorptionFactor {
        public Compound Compound { get; set; }
        public string RouteTypeString { get; set; }
        public double AbsorptionFactor { get; set; }

        public ExposurePathType ExposureRoute {
            get {
                return ExposurePathTypeConverter.FromString(this.RouteTypeString);
            }
        }
    }
}
