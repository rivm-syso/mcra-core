using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksHazardExposureRatioSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<SingleValueRisksHazardExposureRatioRecord> Records { get; set; }
        public bool IsInversDistribution { get; set; }
        public bool UseAdjustmentFactor { get; set; }
        public bool UseAdjustmentFactorBackground { get; set; }

        public void Summarize(
            ICollection<SingleValueRiskCalculationResult> results,
            double adjustmentFactorExposure,
            double adjustmentFactorHazard,
            double focalCommodityContribution,
            bool useAdjustmentFactor,
            bool useAdjustmentFactorBackground,
            Compound referenceSubstance,
            bool isInversDistribution,
            double percentage
        ) {
            IsInversDistribution = isInversDistribution;
            UseAdjustmentFactor = useAdjustmentFactor;
            UseAdjustmentFactorBackground = useAdjustmentFactorBackground;
            Records = results
                .Select(c => {
                    return new SingleValueRisksHazardExposureRatioRecord() {
                        SubstanceName = referenceSubstance?.Name,
                        SubstanceCode = referenceSubstance?.Code,
                        Percentage = percentage,
                        Risk = c.HazardExposureRatio,
                        AdjustmentFactor = adjustmentFactorExposure * adjustmentFactorHazard * (1 - focalCommodityContribution) + focalCommodityContribution,
                        Risks = new List<double>(),
                        AdjustedRisks = new List<double>(),
                        ReferenceValueExposure = c.Exposure,
                        HazardCharacterisation = c.HazardCharacterisation,
                        ReferenceValueExposures = new List<double>()
                    };
                })
                .ToList();
        }

        public SingleValueSummaryRecord GetSingleValueSummary() {
            var record = new SingleValueSummaryRecord() {
                RiskValue = Records[0].AdjustedRisk,
                MedianRiskValue = Records[0].MedianAdjustedRisk,
                LowerRiskValue = Records[0].PLowerAdjustedRisk,
                UpperRiskValue = Records[0].PUpperAdjustedRisk,
            };
            return record;
        }

        public void SummarizeUncertainty(
            ICollection<SingleValueRiskCalculationResult> results,
            double adjustmentFactorExposure,
            double adjustmentFactorHazard,
            double focalCommodityContribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var record in Records) {
                record.UncertaintyLowerLimit = uncertaintyLowerBound;
                record.UncertaintyUpperLimit = uncertaintyUpperBound;
                record.Risks.Add(results.First().HazardExposureRatio);
                record.ReferenceValueExposures.Add(results.First().Exposure);
                record.AdjustedRisks.Add(results.First().HazardExposureRatio * (adjustmentFactorExposure * adjustmentFactorHazard * (1 - focalCommodityContribution) + focalCommodityContribution));
            }
        }
    }
}
