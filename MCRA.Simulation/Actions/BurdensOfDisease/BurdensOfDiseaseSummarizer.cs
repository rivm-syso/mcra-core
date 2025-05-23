using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.BurdensOfDisease {
    public enum BurdensOfDiseaseSections {
        BurdensOfDiseaseSummarySection
    }
    public sealed class BurdensOfDiseaseSummarizer : ActionResultsSummarizerBase<IIBurdensOfDiseaseActionResult> {

        public override ActionType ActionType => ActionType.BurdensOfDisease;

        public override void Summarize(ActionModuleConfig sectionConfig, IIBurdensOfDiseaseActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<BurdensOfDiseaseSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new BurdensOfDiseaseSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.BurdensOfDisease);
            subHeader.SaveSummarySection(section);
        }
    }
}
