using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksHazardIndexSection : ActionSummaryBase {

        public List<SingleValueRisksHazardIndexRecord> Records { get; set; }
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
                    return new SingleValueRisksHazardIndexRecord() {
                        SubstanceName = referenceSubstance.Name,
                        SubstanceCode = referenceSubstance.Code,
                        Percentage = percentage,
                        HazardIndex = c.HazardQuotient,
                        AdjustmentFactor = adjustmentFactorExposure * adjustmentFactorHazard * (1 - focalCommodityContribution) + focalCommodityContribution,
                        HazardIndices = new List<double>(),
                        AdjustedHazardIndices = new List<double>(),
                        ReferenceValueExposure = c.Exposure,
                        HazardCharacterisation= c.HazardCharacterisation,
                        ReferenceValueExposures = new List<double>(),
                    };
                })
                .ToList();
        }
        public SingleValueSummaryRecord GetSingleValueSummary() {
            var record = new SingleValueSummaryRecord() {
                RiskValue = Records[0].AdjustedHazardIndex,
                MedianRiskValue = Records[0].MedianAdjustedHazardIndex,
                LowerRiskValue = Records[0].PLowerAdjustedHazardIndex,
                UpperRiskValue = Records[0].PUpperAdjustedHazardIndex,
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
                record.HazardIndices.Add(results.First().HazardQuotient);
                record.ReferenceValueExposures.Add(results.First().Exposure);
                record.AdjustedHazardIndices.Add(results.First().HazardQuotient / (adjustmentFactorExposure * adjustmentFactorHazard * (1 - focalCommodityContribution) + focalCommodityContribution));
            }
        }
    }
}
