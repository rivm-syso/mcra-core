using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksMarginOfExposureSection : ActionSummaryBase {
        public List<SingleValueRisksMarginOfExposureRecord> Records { get; set; }
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
                    return new SingleValueRisksMarginOfExposureRecord() {
                        SubstanceName = referenceSubstance.Name,
                        SubstanceCode = referenceSubstance.Code,
                        Percentage = percentage,
                        MarginOfExposure = c.MarginOfExposure,
                        AdjustmentFactor = adjustmentFactorExposure * adjustmentFactorHazard * (1 - focalCommodityContribution) + focalCommodityContribution,
                        MarginOfExposures = new List<double>(),
                        AdjustedMarginOfExposures = new List<double>(),
                        ReferenceValueExposure = c.Exposure,
                        HazardCharacterisation = c.HazardCharacterisation,
                        ReferenceValueExposures = new List<double>()
                    };
                })
                .ToList();
        }

        public SingleValueSummaryRecord GetSingleValueSummary() {
            var record = new SingleValueSummaryRecord() {
                RiskValue = Records[0].AdjustedMarginOfExposure,
                MedianRiskValue = Records[0].MedianAdjustedMarginOfExposure,
                LowerRiskValue = Records[0].PLowerAdjustedMarginOfExposure,
                UpperRiskValue = Records[0].PUpperAdjustedMarginOfExposure,
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
                record.MarginOfExposures.Add(results.First().MarginOfExposure);
                record.ReferenceValueExposures.Add(results.First().Exposure);
                record.AdjustedMarginOfExposures.Add(results.First().MarginOfExposure * (adjustmentFactorExposure * adjustmentFactorHazard * (1 - focalCommodityContribution) + focalCommodityContribution));
            }
        }
    }
}
