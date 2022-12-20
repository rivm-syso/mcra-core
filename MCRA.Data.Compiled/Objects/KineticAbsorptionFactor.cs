using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class KineticAbsorptionFactor {
        public Compound Compound { get; set; }
        public string RouteTypeString { get; set; }
        public double AbsorptionFactor { get; set; }

        public ExposureRouteType ExposureRoute {
            get {
                return ExposureRouteTypeConverter.FromString(this.RouteTypeString);
            }
        }
    }
}
