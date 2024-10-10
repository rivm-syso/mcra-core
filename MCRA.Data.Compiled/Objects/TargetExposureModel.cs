using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class TargetExposureModel : StrongEntity {

        public Compound Compound { get; set; }

        public string ExposureUnitString { get; set; }

        public Dictionary<double, TargetExposurePercentile> TargetExposurePercentiles { get; set; }

        public ExternalExposureUnit ExposureUnit {
            get {
                if (!string.IsNullOrEmpty(ExposureUnitString)) {
                    return ExternalExposureUnitConverter.FromString(ExposureUnitString);
                }
                return ExternalExposureUnit.mgPerKgBWPerDay;
            }
        }
    }
}
