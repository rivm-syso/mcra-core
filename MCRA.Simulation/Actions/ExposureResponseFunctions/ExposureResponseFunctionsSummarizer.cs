using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ExposureResponseFunctions {
    public enum ExposureResponseFunctionsSections {
        ErfSummarySection
    }
    public sealed class ExposureResponseFunctionsSummarizer : ActionResultsSummarizerBase<IExposureResponseFunctionsActionResult> {

        public override ActionType ActionType => ActionType.ExposureResponseFunctions;

        public override void Summarize(ActionModuleConfig sectionConfig, IExposureResponseFunctionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ExposureResponseFunctionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new ExposureResponseFunctionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.ExposureResponseFunctions);
            subHeader.SaveSummarySection(section);
        }
    }
}
