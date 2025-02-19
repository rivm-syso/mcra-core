using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.BaselineBodIndicators { 
    public enum BaselineBodIndicatorsSections {
        BaselineBodIndicatorSummarySection
    }
    public sealed class BaselineBodIndicatorsSummarizer : ActionResultsSummarizerBase<IBaselineBodIndicatorsActionResult> {

        public override ActionType ActionType => ActionType.BaselineBodIndicators;

        public override void Summarize(ActionModuleConfig sectionConfig, IBaselineBodIndicatorsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<BaselineBodIndicatorsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new BaselineBodIndicatorsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.BaselineBodIndicators);
            subHeader.SaveSummarySection(section);
        }
    }
}
