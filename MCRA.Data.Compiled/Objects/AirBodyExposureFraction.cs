using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class AirBodyExposureFraction : IExposureDistribution {
        public string idSubgroup { get; set; }
        public double? AgeLower { get; set; }
        public GenderType Sex { get; set; }
        public double Value { get; set; }
        public AirBodyExposureFractionDistributionType DistributionType { get; set; }
        public string ExposureDistributionTypeString { get { return DistributionType.GetShortDisplayName(); } }
        public double? CvVariability { get; set; }
    }
}
