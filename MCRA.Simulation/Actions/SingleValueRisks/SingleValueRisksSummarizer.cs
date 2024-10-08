using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.SingleValueRisks {
    public enum SingleValueRisksSections { 
    }
    public sealed class SingleValueRisksSummarizer : ActionModuleResultsSummarizer<SingleValueRisksModuleConfig, SingleValueRisksActionResult> {

        public SingleValueRisksSummarizer(SingleValueRisksModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig outputConfig,
            SingleValueRisksActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueRisksSections>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SingleValueRisksSection() { 
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);

            var subOrder = 0;
            if (_configuration.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromSingleValues) {
                summarizeSingleValueRisksBySourceSubstance(result, subHeader, subOrder++);
            } else {
                var singleValueSummaryRecord = summarizeDetailsSingleValueRisks(
                    result,
                    data.ReferenceSubstance,
                    _configuration.Percentage,
                    _configuration.IsInverseDistribution,
                    _configuration.RiskMetricType,
                    _configuration.UseAdjustmentFactors,
                    _configuration.UseBackgroundAdjustmentFactor,
                    subHeader,
                    4
                );

                if (_configuration.UseAdjustmentFactors
                        && (_configuration.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None
                        || _configuration.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None)
                    ) {
                    summarizeSingleValueRisksAdjustmentFactors(
                        result,
                        _configuration.IsInverseDistribution,
                        _configuration.UseAdjustmentFactors,
                        _configuration.UseBackgroundAdjustmentFactor,
                        _configuration.ExposureAdjustmentFactorDistributionMethod,
                        _configuration.ExposureParameterA,
                        _configuration.ExposureParameterB,
                        _configuration.ExposureParameterC,
                        _configuration.ExposureParameterD,
                        _configuration.HazardAdjustmentFactorDistributionMethod,
                        _configuration.HazardParameterA,
                        _configuration.HazardParameterB,
                        _configuration.HazardParameterC,
                        _configuration.HazardParameterD,
                        subHeader,
                        4
                    );
                }

                section.Summarize(
                    _configuration.IsInverseDistribution,
                    _configuration.Percentage,
                    singleValueSummaryRecord,
                    _configuration.RiskMetricType,
                    _configuration.UseAdjustmentFactors,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        public void SummarizeUncertain(
            SingleValueRisksActionResult result,
            SectionHeader header
        ) {
            var singleValueSummaryRecord = summarizeDetailsSingleValueRisksUncertainty(
                result,
                _configuration.RiskMetricType,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                header
            );
            if (singleValueSummaryRecord != null) {
                summarizeSingleValueRiskSummaryUncertainty(
                    singleValueSummaryRecord,
                    _configuration.RiskMetricType,
                    _configuration.Percentage,
                    _configuration.IsInverseDistribution,
                    _configuration.UseAdjustmentFactors,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    header
                );
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var lowerPercentage = _configuration.UncertaintyLowerBound;
            var upperPercentage = _configuration.UncertaintyUpperBound;
            var result = new List<ActionSummaryUnitRecord> {
                new("RiskMetric", _configuration.RiskMetricType.GetDisplayName()),
                new("RiskMetricShort", _configuration.RiskMetricType.GetShortDisplayName()),
                new("SingleValueExposuresUnit", data.SingleValueDietaryExposureUnit?.GetShortDisplayName() ?? "-"),
                new("LowerConfidenceBound", $"p{lowerPercentage:#0.##}"),
                new("UpperConfidenceBound", $"p{upperPercentage:#0.##}")
            };
            return result;
        }

        private void summarizeSingleValueRisksBySourceSubstance(
            SingleValueRisksActionResult result,
            SectionHeader header,
            int order
        ) {
            var section = new SingleValueRisksBySourceSubstanceSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Single value risks by source and substance", order++);
            section.Summarize(result.SingleValueRiskEstimates);
            subHeader.SaveSummarySection(section);
        }

        private SingleValueSummaryRecord summarizeDetailsSingleValueRisks(
            SingleValueRisksActionResult result,
            Compound referenceSubstance,
            double percentage,
            bool isInverseDistribution,
            RiskMetricType riskMetric,
            bool useAdjustmentFactors,
            bool useAdjustmentFactorsBackground,
            SectionHeader header,
            int order
        ) {
            if (riskMetric == RiskMetricType.HazardExposureRatio) {
                var section = new SingleValueRisksHazardExposureRatioSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Details", order++);
                section.Summarize(
                    result.SingleValueRiskEstimates,
                    result.AdjustmentFactorExposure,
                    result.AdjustmentFactorHazard,
                    result.FocalCommodityContribution,
                    useAdjustmentFactors,
                    useAdjustmentFactorsBackground,
                    referenceSubstance,
                    isInverseDistribution,
                    percentage
                );
                subHeader.SaveSummarySection(section);
                return section.GetSingleValueSummary();
            } else {
                var section = new SingleValueRisksExposureHazardRatioSection();
                var subHeader = header.AddSubSectionHeaderFor(section, "Details", order++);
                section.Summarize(
                    result.SingleValueRiskEstimates,
                    result.AdjustmentFactorExposure,
                    result.AdjustmentFactorHazard,
                    result.FocalCommodityContribution,
                    useAdjustmentFactors,
                    useAdjustmentFactorsBackground,
                    referenceSubstance,
                    isInverseDistribution,
                    percentage
                );
                subHeader.SaveSummarySection(section);
                return section.GetSingleValueSummary();
            }
        }

        private void summarizeSingleValueRisksAdjustmentFactors(
            SingleValueRisksActionResult result,
            bool isInverseDistribution,
            bool useAdjustmentFactors,
            bool useAdjustmentFactorsBackground,
            AdjustmentFactorDistributionMethod exposureAdjustmentFactorDistributionMethod,
            double exposureParameterA,
            double exposureParameterB,
            double exposureParameterC,
            double exposureParameterD,
            AdjustmentFactorDistributionMethod hazardAdjustmentFactorDistributionMethod,
            double hazardParameterA,
            double hazardParameterB,
            double hazardParameterC,
            double hazardParameterD,
            SectionHeader header,
            int order
        ) {
            var section = new SingleValueRisksAdjustmentFactorsSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Single value risks adjustment factors", order++);
            section.Summarize(
                result.AdjustmentFactorExposure,
                result.AdjustmentFactorHazard,
                result.FocalCommodityContribution,
                useAdjustmentFactors,
                useAdjustmentFactorsBackground,
                isInverseDistribution,
                exposureAdjustmentFactorDistributionMethod,
                exposureParameterA,
                exposureParameterB,
                exposureParameterC,
                exposureParameterD,
                hazardAdjustmentFactorDistributionMethod,
                hazardParameterA,
                hazardParameterB,
                hazardParameterC,
                hazardParameterD
            );
            subHeader.SaveSummarySection(section);
        }

        private SingleValueSummaryRecord summarizeDetailsSingleValueRisksUncertainty(
            SingleValueRisksActionResult result,
            RiskMetricType riskMetric,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            SectionHeader header
        ) {
            if (riskMetric == RiskMetricType.HazardExposureRatio) {
                var subHeader = header.GetSubSectionHeader<SingleValueRisksHazardExposureRatioSection>();
                if (subHeader == null) {
                    return null;
                }
                var outputSummary = (SingleValueRisksHazardExposureRatioSection)subHeader.GetSummarySection();
                if (outputSummary == null) {
                    return null;
                }

                var section = subHeader.GetSummarySection() as SingleValueRisksHazardExposureRatioSection;
                section.SummarizeUncertainty(
                    result.SingleValueRiskEstimates,
                    result.AdjustmentFactorExposure,
                    result.AdjustmentFactorHazard,
                    result.FocalCommodityContribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
                return section.GetSingleValueSummary();
            } else {
                var subHeader = header.GetSubSectionHeader<SingleValueRisksExposureHazardRatioSection>();
                if (subHeader == null) {
                    return null;
                }
                var outputSummary = (SingleValueRisksExposureHazardRatioSection)subHeader.GetSummarySection();
                if (outputSummary == null) {
                    return null;
                }

                var section = subHeader.GetSummarySection() as SingleValueRisksExposureHazardRatioSection;
                section.SummarizeUncertainty(
                    result.SingleValueRiskEstimates,
                    result.AdjustmentFactorExposure,
                    result.AdjustmentFactorHazard,
                    result.FocalCommodityContribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
                return section.GetSingleValueSummary();
            }
        }

        private void summarizeSingleValueRiskSummaryUncertainty(
            SingleValueSummaryRecord singleValueSummaryRecord,
            RiskMetricType riskMetric,
            double percentage,
            bool isInverseDistribution,
            bool isAdjustment,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<SingleValueRisksSection>();
            if (subHeader != null) {
                var outputSummary = (SingleValueRisksSection)subHeader.GetSummarySection();
                if (outputSummary == null) {
                    return;
                }

                var section = subHeader.GetSummarySection() as SingleValueRisksSection;
                section.Summarize(
                    isInverseDistribution,
                    percentage,
                    singleValueSummaryRecord,
                    riskMetric,
                    isAdjustment,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
