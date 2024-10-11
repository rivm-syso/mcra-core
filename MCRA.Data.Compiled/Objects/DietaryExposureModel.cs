using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DietaryExposureModel : StrongEntity {
        public Compound Compound { get; set; }

        public Dictionary<double, DietaryExposurePercentile> DietaryExposurePercentiles { get; set; }

        public ExternalExposureUnit ExposureUnit { get; set; } = ExternalExposureUnit.mgPerKgBWPerDay;
    }
}
