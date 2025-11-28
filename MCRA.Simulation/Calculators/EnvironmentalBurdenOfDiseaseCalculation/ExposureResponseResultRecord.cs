using MCRA.Simulation.Calculators.ExposureResponseFunctions;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class ExposureResponseResultRecord {
        public IExposureResponseModel ExposureResponseModel { get; set; }
        public bool IsErfDefinedExposureBin { get; set; }
        public int ExposureBinId { get; set; }
        public PercentileInterval PercentileInterval { get; set; }
        public ExposureInterval ExposureInterval { get; set; }
        public double ExposureLevel { get; set; }
        public double PercentileSpecificRisk { get; set; }
    }
}
