using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.BurdensOfDisease {
    public enum BurdensOfDiseaseSections {
        BurdensOfDiseaseSummarySection,
        BodIndicatorConversionsSummarySection
    }
    public sealed class BurdensOfDiseaseSummarizer : ActionResultsSummarizerBase<IIBurdensOfDiseaseActionResult> {

        public override ActionType ActionType => ActionType.BurdensOfDisease;

        public override void Summarize(ActionModuleConfig sectionConfig, IIBurdensOfDiseaseActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<BurdensOfDiseaseSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());

            if (outputSettings.ShouldSummarize(BurdensOfDiseaseSections.BodIndicatorConversionsSummarySection)
                && data.BurdensOfDisease != null
            ) {
                summarizeBurdensOfDisease(
                    data,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(BurdensOfDiseaseSections.BodIndicatorConversionsSummarySection)
                && data.BodIndicatorConversions != null
            ) {
                summarizeBodIncicatorConversions(
                    data,
                    subHeader,
                    order++
                );
            }
        }

        private void summarizeBurdensOfDisease(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new BurdensOfDiseaseSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.BurdensOfDisease);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeBodIncicatorConversions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new BodIndicatorConversionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "BoD indicator conversions", order);
            section.Summarize(data.BodIndicatorConversions);
            subHeader.SaveSummarySection(section);
        }
    }
}
