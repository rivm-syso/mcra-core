using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    [Serializable]
    public class KineticModelInstanceParameter {
        public string IdModelInstance { get; set; }
        public string Parameter { get; set; }
        public string Description { get; set; }
        public double Value { get; set; }
        public double? CvVariability { get; set; }
        public double? CvUncertainty { get; set; }

        public ProbabilityDistribution DistributionType { get; set; }

        public KineticModelInstanceParameter Clone(double value) {
            var parameter = new KineticModelInstanceParameter() {
                IdModelInstance = IdModelInstance,
                Description = Description,
                CvUncertainty = CvUncertainty,
                CvVariability = CvVariability,
                DistributionType = DistributionType,
                Parameter = Parameter,
                Value = value,
            };
            return parameter;
        }
    }
}