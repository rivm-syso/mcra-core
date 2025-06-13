﻿using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public enum EnvironmentalBurdenOfDiseaseSections {
        AttributableBodSummarySection,
        ExposureResponseFunctionSummarySection
    }
    public sealed class EnvironmentalBurdenOfDiseaseSummarizer : ActionModuleResultsSummarizer<EnvironmentalBurdenOfDiseaseModuleConfig, EnvironmentalBurdenOfDiseaseActionResult> {
        public EnvironmentalBurdenOfDiseaseSummarizer(EnvironmentalBurdenOfDiseaseModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            EnvironmentalBurdenOfDiseaseActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<EnvironmentalBurdenOfDiseaseSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new EnvironmentalBurdenOfDiseaseSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(
                result.EnvironmentalBurdenOfDiseases
            );
            subHeader.SaveSummarySection(section);

            var subOrder = 0;

            // Table of attributable EBDs
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.AttributableBodSummarySection)
                && data.EnvironmentalBurdenOfDiseases.Count > 0
            ) {
                summarizeAttributableBod(
                    result.EnvironmentalBurdenOfDiseases,
                    subHeader,
                    subOrder++
                );
            }

            // Plot of exposure response function
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.ExposureResponseFunctionSummarySection)
                && data.EnvironmentalBurdenOfDiseases.Count > 0
            ) {
                summarizeExposureResponseFunction(
                    result,
                    data,
                    subHeader,
                    subOrder++
                );
            }
        }

        public void SummarizeUncertain(
            EnvironmentalBurdenOfDiseaseActionResult result,
            ActionData data,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<EnvironmentalBurdenOfDiseaseSummarySection>();
            if (subHeader == null) {
                return;
            } else {
                var section = subHeader.GetSummarySection() as EnvironmentalBurdenOfDiseaseSummarySection;
                section.SummarizeUncertainty(
                    result.EnvironmentalBurdenOfDiseases,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound
                );
                subHeader.Units = collectUnits(data.EnvironmentalBurdenOfDiseases);
                subHeader.SaveSummarySection(section);
            }

            if (data.EnvironmentalBurdenOfDiseases.Count > 0) {
                summarizeAttributableBodUncertainty(
                    result.EnvironmentalBurdenOfDiseases,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    subHeader
                );
            }

            if (data.EnvironmentalBurdenOfDiseases.Count > 0 &&
                data.ExposureResponseFunctionModels.Count > 0) {
                summarizeExposureResponseFunctionUncertainty(
                    data.ExposureResponseFunctionModels,
                    data.EnvironmentalBurdenOfDiseases,
                    _configuration.UncertaintyLowerBound,
                    _configuration.UncertaintyUpperBound,
                    subHeader
                );
            }
        }

        private void summarizeAttributableBod(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            SectionHeader header,
            int order
        ) {
            var section = new AttributableBodSummarySection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.AttributableBodSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Attributable Burden of disease",
                order
            );
            section.Summarize(environmentalBurdenOfDiseases);
            subHeader.Units = collectUnits(environmentalBurdenOfDiseases);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeAttributableBodUncertainty(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<AttributableBodSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as AttributableBodSummarySection;
                section.SummarizeUncertainty(
                    environmentalBurdenOfDiseases,
                    lowerBound,
                    upperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private void summarizeExposureResponseFunction(
            EnvironmentalBurdenOfDiseaseActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureResponseFunctionSummarySection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.ExposureResponseFunctionSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposure response function",
                order
            );
            section.Summarize(
                data.ExposureResponseFunctionModels,
                data.EnvironmentalBurdenOfDiseases
            );
            subHeader.SaveSummarySection(section);
        }
        private void summarizeExposureResponseFunctionUncertainty(
            ICollection<IExposureResponseFunctionModel> exposureResponseFunctionModels,
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            double lowerBound,
            double upperBound,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<ExposureResponseFunctionSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as ExposureResponseFunctionSummarySection;
                section.SummarizeUncertainty(
                    exposureResponseFunctionModels,
                    environmentalBurdenOfDiseases,
                    lowerBound,
                    upperBound
                );
                subHeader.SaveSummarySection(section);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            var result = new List<ActionSummaryUnitRecord> {
                new ("EffectMetric", environmentalBurdenOfDiseases.FirstOrDefault().ExposureResponseFunction.EffectMetric.GetShortDisplayName()),
                new ("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new ("UpperBound", $"p{_configuration.UncertaintyUpperBound}")
            };
            return result;
        }
    }
}
