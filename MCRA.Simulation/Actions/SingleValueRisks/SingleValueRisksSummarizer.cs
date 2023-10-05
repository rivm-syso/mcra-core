using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks;

namespace MCRA.Simulation.Actions.SingleValueRisks {
    public enum SingleValueRisksSections { 
    }
    public sealed class SingleValueRisksSummarizer : ActionResultsSummarizerBase<SingleValueRisksActionResult> {

        public override ActionType ActionType => ActionType.SingleValueRisks;

        public override void Summarize(
                ProjectDto project,
                SingleValueRisksActionResult result,
                ActionData data,
                SectionHeader header,
                int order
            ) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueRisksSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new SingleValueRisksSection() { 
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);

            var subOrder = 0;
            if (project.EffectModelSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromSingleValues) {
                summarizeSingleValueRisksBySourceSubstance(result, subHeader, subOrder++);
            } else {
                var singleValueSummaryRecord = summarizeDetailsSingleValueRisks(
                    result,
                    data.ReferenceSubstance,
                    project.EffectModelSettings.Percentage,
                    project.EffectModelSettings.IsInverseDistribution,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.UseAdjustmentFactors,
                    project.EffectModelSettings.UseBackgroundAdjustmentFactor,
                    subHeader,
                    4
                );

                if (project.EffectModelSettings.UseAdjustmentFactors
                        && (project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None
                        || project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod != AdjustmentFactorDistributionMethod.None)
                    ) {
                    summarizeSingleValueRisksAdjustmentFactors(
                        result,
                        project.EffectModelSettings.IsInverseDistribution,
                        project.EffectModelSettings.UseAdjustmentFactors,
                        project.EffectModelSettings.UseBackgroundAdjustmentFactor,
                        project.EffectModelSettings.ExposureAdjustmentFactorDistributionMethod,
                        project.EffectModelSettings.ExposureParameterA,
                        project.EffectModelSettings.ExposureParameterB,
                        project.EffectModelSettings.ExposureParameterC,
                        project.EffectModelSettings.ExposureParameterD,
                        project.EffectModelSettings.HazardAdjustmentFactorDistributionMethod,
                        project.EffectModelSettings.HazardParameterA,
                        project.EffectModelSettings.HazardParameterB,
                        project.EffectModelSettings.HazardParameterC,
                        project.EffectModelSettings.HazardParameterD,
                        subHeader,
                        4
                    );
                }

                section.Summarize(
                    project.EffectModelSettings.IsInverseDistribution,
                    project.EffectModelSettings.Percentage,
                    singleValueSummaryRecord,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.UseAdjustmentFactors,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        public void SummarizeUncertain(
                ProjectDto project,
                SingleValueRisksActionResult result,
                ActionData data,
                SectionHeader header
            ) {
            var singleValueSummaryRecord = summarizeDetailsSingleValueRisksUncertainty(
                result,
                project.EffectModelSettings.RiskMetricType,
                project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                header
            );
            if (singleValueSummaryRecord != null) {
                summarizeSingleValueRiskSummaryUncertainty(
                    singleValueSummaryRecord,
                    project.EffectModelSettings.RiskMetricType,
                    project.EffectModelSettings.Percentage,
                    project.EffectModelSettings.IsInverseDistribution,
                    project.EffectModelSettings.UseAdjustmentFactors,
                    project.UncertaintyAnalysisSettings.UncertaintyLowerBound,
                    project.UncertaintyAnalysisSettings.UncertaintyUpperBound,
                    header
                );
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var lowerPercentage = project.UncertaintyAnalysisSettings.UncertaintyLowerBound;
            var upperPercentage = project.UncertaintyAnalysisSettings.UncertaintyUpperBound;
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("RiskMetric", project.EffectModelSettings.RiskMetricType.GetDisplayName()),
                new ActionSummaryUnitRecord("RiskMetricShort", project.EffectModelSettings.RiskMetricType.GetShortDisplayName()),
                new ActionSummaryUnitRecord("HazardCharacterisationsUnit", data.HazardCharacterisationsUnit?.GetShortDisplayName() ?? "-"),
                new ActionSummaryUnitRecord("SingleValueExposuresUnit", data.SingleValueDietaryExposureUnit?.GetShortDisplayName() ?? "-"),
                new ActionSummaryUnitRecord("LowerConfidenceBound", $"p{lowerPercentage:#0.##}"),
                new ActionSummaryUnitRecord("UpperConfidenceBound", $"p{upperPercentage:#0.##}")
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
            if (riskMetric == RiskMetricType.MarginOfExposure) {
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
            if (riskMetric == RiskMetricType.MarginOfExposure) {
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
