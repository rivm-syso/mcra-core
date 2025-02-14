using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease {
    public enum EnvironmentalBurdenOfDiseaseSections {
        EbdSummaryTableSection,
        AttributableBodSummarySection,
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
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.AttributableBodSummarySection)
                && data.EnvironmentalBurdenOfDiseases.Count > 0
            ) {
                summarizeAttributableBod(
                    data.EnvironmentalBurdenOfDiseases,
                    subHeader,
                    subOrder++
                );
            }

            // Plot of exposure effect function
            if (outputSettings.ShouldSummarize(EnvironmentalBurdenOfDiseaseSections.ExposureEffectFunctionSummarySection)
                && data.EnvironmentalBurdenOfDiseases.Count > 0
            ) {
                summarizeExposureEffectFunction(
                    data.EnvironmentalBurdenOfDiseases,
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

        private void summarizeExposureEffectFunction(
            List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases,
            SectionHeader header,
            int order
        ) {
            var section = new ExposureEffectFunctionSummarySection() {
                SectionLabel = getSectionLabel(EnvironmentalBurdenOfDiseaseSections.ExposureEffectFunctionSummarySection)
            };

            section.Summarize(
                environmentalBurdenOfDiseases);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposure effect function",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(List<EnvironmentalBurdenOfDiseaseResultRecord> environmentalBurdenOfDiseases) {
            var result = new List<ActionSummaryUnitRecord> {
                new("EffectMetric", environmentalBurdenOfDiseases.First().ExposureEffectFunction.EffectMetric.GetShortDisplayName())
            };
            return result;
        }
    }
}
