using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksAdjustmentFactorsSection : SummarySection {

        public List<SingleValueRisksAdjustmentFactorRecord> AdjustmentFactorRecords { get; set; }
        public bool IsInversDistribution { get; set; }

        public AdjustmentFactorDistributionMethod ExposureAdjustmentFactorDistributionMethod { get; set; }
        public double ExposureParameterA { get; set; }
        public double ExposureParameterB { get; set; }
        public double ExposureParameterC { get; set; }
        public double ExposureParameterD { get; set; }
        public AdjustmentFactorDistributionMethod HazardAdjustmentFactorDistributionMethod { get; set; }
        public double HazardParameterA { get; set; }
        public double HazardParameterB { get; set; }
        public double HazardParameterC { get; set; }
        public double HazardParameterD { get; set; }
        public bool UseAdjustmentFactor { get; set; }
        public bool UseAdjustmentFactorBackground { get; set; }

        public void Summarize(
            double adjustmentFactorExposure,
            double adjustmentFactorHazard,
            double focalCommodityContribution,
            bool useAdjustmentFactor,
            bool useAdjustmentFactorBackground,
            bool isInversDistribution,
            AdjustmentFactorDistributionMethod exposureAdjustmentFactorDistributionMethod,
            double exposureParameterA,
            double exposureParameterB,
            double exposureParameterC,
            double exposureParameterD,
            AdjustmentFactorDistributionMethod hazardAdjustmentFactorDistributionMethod,
            double hazardParameterA,
            double hazardParameterB,
            double hazardParameterC,
            double hazardParameterD
        ) {
            ExposureAdjustmentFactorDistributionMethod = exposureAdjustmentFactorDistributionMethod;
            ExposureParameterA = exposureParameterA;
            ExposureParameterB = exposureParameterB;
            ExposureParameterC = exposureParameterC;
            ExposureParameterD = exposureParameterD;
            HazardAdjustmentFactorDistributionMethod = hazardAdjustmentFactorDistributionMethod;
            HazardParameterA = hazardParameterA;
            HazardParameterB = hazardParameterB;
            HazardParameterC = hazardParameterC;
            HazardParameterD = hazardParameterD;
            IsInversDistribution = isInversDistribution;
            UseAdjustmentFactor = useAdjustmentFactor;
            UseAdjustmentFactorBackground = useAdjustmentFactorBackground;

            AdjustmentFactorRecords = [ new SingleValueRisksAdjustmentFactorRecord() {
                    AdjustmentFactorExposure = adjustmentFactorExposure,
                    AdjustmentFactorHazard = adjustmentFactorHazard,
                    BackgroundContribution = 1 - focalCommodityContribution
                }
            ];
        }
    }
}
