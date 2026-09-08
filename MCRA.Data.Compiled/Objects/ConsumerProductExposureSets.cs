using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductExposureSet {
        public string IdExposureSet { get; set; }
        public string IdAssessment { get; set; }
        public Compound Substance { get; set; }
        public List<ConsumerProduct> Products { get; set; }
        public ConsumerProductExposureEstimateType EstimateType { get; set; }
        public List<ExposureRoute> ExposureRoutes { get; set; }
        public double? PercentageZeros { get; set; }
        public double? AbsorptionFactor { get; set; } = 1;
        public ConsumerProductExposureEstimateUnit ConsumerProductExposureEstimateUnit { get; set; }
        public List<ConsumerProductExposurePercentile> ExposurePercentiles { get; set; }
    }
}
