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
            subHeader.SaveSummarySection(section);
            var subOrder = 0;
            summarizeEff(
                data.ExposureEffectFunctions,
                subHeader,
                subOrder++
            );
        }

        private void summarizeEff(
            List<ExposureEffectFunction> exposureEffectFunctions,
            SectionHeader header,
            int order
         ) {
            var section = new EefSummaryTableSection() {
                SectionLabel = getSectionLabel(ExposureEffectFunctionsSections.EefSummarySection)
            };

            section.Summarize(exposureEffectFunctions);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Exposure Effect Function Summary",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
