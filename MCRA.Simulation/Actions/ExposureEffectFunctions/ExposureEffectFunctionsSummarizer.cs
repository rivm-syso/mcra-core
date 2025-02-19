using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ExposureEffectFunctions {
    public enum ExposureEffectFunctionsSections {
        EefSummarySection
    }
    public sealed class ExposureEffectFunctionsSummarizer : ActionResultsSummarizerBase<IExposureEffectFunctionsActionResult> {

        public override ActionType ActionType => ActionType.ExposureEffectFunctions;

        public override void Summarize(ActionModuleConfig sectionConfig, IExposureEffectFunctionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ExposureEffectFunctionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new ExposureEffectFunctionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Summarize(data.ExposureEffectFunctions);
            subHeader.SaveSummarySection(section);
        }
    }
}
