using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public class KineticModelInstanceParameter {
        public string IdModelInstance { get; set; }
        public string Parameter { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
        public string DistributionTypeString { get; set; }
        public double? CvVariability { get; set; }
        public double? CvUncertainty { get; set; }

        public ProbabilityDistribution DistributionType {
            get {
                return ProbabilityDistributionConverter.FromString(DistributionTypeString);
            }
        }

        public KineticModelInstanceParameter Clone(double value) {
            var parameter = new KineticModelInstanceParameter() {
                IdModelInstance = IdModelInstance,
                Description = Description,
                CvUncertainty = CvUncertainty,
                CvVariability = CvVariability,
                DistributionTypeString = DistributionTypeString,
                Parameter = Parameter,
                Value = value,
            };
            return parameter;
        }
    }
}