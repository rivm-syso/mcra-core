using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public interface IExposureDistribution {
        public string idSubgroup { get; }
        public double? AgeLower { get; }
        public GenderType Sex { get; }
        public double Value { get; }
        public string ExposureDistributionTypeString { get; }
        public double? CvVariability { get; }
    }
}
