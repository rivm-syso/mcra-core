using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.QsarMembershipModels {
    public enum QsarMembershipModelsSections {
        //No sub-sections
    }
    public sealed class QsarMembershipModelsSummarizer : ActionResultsSummarizerBase<IQsarMembershipModelsActionResult> {

        public override ActionType ActionType => ActionType.QsarMembershipModels;

        public override void Summarize(ProjectDto project, IQsarMembershipModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<QsarMembershipModelsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new QsarMembershipModelsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.QsarMembershipModels, data.AllCompounds.ToHashSet());
            subHeader.SaveSummarySection(section);
            if (data.QsarMembershipModels.Count > 1) {
                var section1 = new QsarMembershipModelCorrelationsSection();
                section1.Summarize(data.QsarMembershipModels, data.AllCompounds.ToHashSet());
                var subHeader1 = subHeader.AddSubSectionHeaderFor(section1, "QSAR model correlations", order);
                subHeader1.SaveSummarySection(section1);
            }
        }
    }
}
