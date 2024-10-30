using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public enum EnvironmentalBurdenOfDiseaseSections {
        EbdSummaryTableSection,
        AttributableEbdSummarySection,
        ExposureEffectFunctionSummarySection
    }
    public sealed class EnvironmentalBurdenOfDiseaseSummarizer : ActionResultsSummarizerBase<EnvironmentalBurdenOfDiseaseActionResult> {

        public override ActionType ActionType => ActionType.EnvironmentalBurdenOfDisease;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            EnvironmentalBurdenOfDiseaseActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<EnvironmentalBurdenOfDiseaseSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var outputSummary = new EnvironmentalBurdenOfDiseaseSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(outputSummary, ActionType.GetDisplayName(), order);
            subHeader.SaveSummarySection(outputSummary);
            var subOrder = 0;

            // EBD summary table
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.EbdSummaryTableSection)) {
                summarizeEbd(
                    subHeader,
                    subOrder++
                );
            }

            // Table of attributable EBDs
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.AttributableEbdSummarySection)
                && data.AttributableEbds.Count > 0
            ) {
                summarizeAttributableEbd(
                    data.AttributableEbds,
                    subHeader,
                    subOrder++
                );
            }

            // Plot of exposure effect function
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.ExposureEffectFunctionSummarySection)
                && data.AttributableEbds.Count > 0
            ) {
                summarizeExposureEffectFunction(
                    data.AttributableEbds,
                    data.ExposureEffects,
                    subHeader,
                    subOrder++
                );
            }
        }

        private void summarizeEbd(SectionHeader header, int order) {
            var section = new EbdSummaryTableSection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.EbdSummaryTableSection)
            };

            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Environmental Burden of Disease Summary Table",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeAttributableEbd(
            List<EnvironmentalBurdenOfDiseaseResultRecord> attributableEbds,
            SectionHeader header,
            int order
        ) {
            var section = new AttributableEbdSummarySection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.AttributableEbdSummarySection)
            };

            section.Summarize(attributableEbds);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Attributable EBDs",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureEffectFunction(
            List<EnvironmentalBurdenOfDiseaseResultRecord> attributableEbds,
            List<ExposureEffectResultRecord> exposureEffects,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureEffectFunctionSummarySection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.ExposureEffectFunctionSummarySection)
            };

            section.Summarize(
                attributableEbds);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposure effect function",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
