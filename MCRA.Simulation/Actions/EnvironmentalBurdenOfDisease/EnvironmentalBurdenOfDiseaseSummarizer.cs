using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public enum EnvironmentalBurdenOfDiseaseSections {
        AttributableBodSummarySection,
        ExposureResponseFunctionSummarySection
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
            var section = new EnvironmentalBurdenOfDiseaseSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.EnvironmentalBurdenOfDiseases);
            subHeader.SaveSummarySection(section);
            var subOrder = 0;

            // Table of attributable EBDs
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.AttributableBodSummarySection)
                && data.EnvironmentalBurdenOfDiseases.Count > 0
            ) {
                summarizeAttributableBod(
                    data.EnvironmentalBurdenOfDiseases,
                    subHeader,
                    subOrder++
                );
            }

            // Plot of exposure response function
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.ExposureResponseFunctionSummarySection)
                && data.EnvironmentalBurdenOfDiseases.Count > 0
            ) {
                summarizeExposureResponseFunction(
                    data.EnvironmentalBurdenOfDiseases,
                    subHeader,
                    subOrder++
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
                "Attributable BoDs",
                order
            );
            section.Summarize(environmentalBurdenOfDiseases);
            subHeader.Units = collectUnits(environmentalBurdenOfDiseases);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeExposureResponseFunction(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureResponseFunctionSummarySection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.ExposureResponseFunctionSummarySection)
            };

            section.Summarize(
                environmentalBurdenOfDiseases);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposure response function",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            var result = new List<ActionSummaryUnitRecord> {
                new("EffectMetric", environmentalBurdenOfDiseases.First().ExposureResponseFunction.EffectMetric.GetShortDisplayName())
            };
            return result;
        }
    }
}
