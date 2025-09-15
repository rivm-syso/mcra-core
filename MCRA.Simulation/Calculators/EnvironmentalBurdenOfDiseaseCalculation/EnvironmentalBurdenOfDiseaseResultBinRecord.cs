namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class EnvironmentalBurdenOfDiseaseResultBinRecord {
        public int ExposureBinId { get; set; }
        public ExposureInterval ExposureBin { get; set; }
        public PercentileInterval ExposurePercentileBin { get; set; }
        public double Exposure { get; set; }
        public double ResponseValue { get; set; }
        public double AttributableFraction { get; set; }
        public double TotalBod { get; set; } 
        public double AttributableBod { get; set; }
        public ExposureResponseResultRecord ExposureResponseResultRecord { get; set; }
        public double CumulativeAttributableBod { get; set; }
        public double CumulativeStandardisedExposedAttributableBod { get; set; }
        public double StandardisedPopulationSize { get; set; }
    }
}
