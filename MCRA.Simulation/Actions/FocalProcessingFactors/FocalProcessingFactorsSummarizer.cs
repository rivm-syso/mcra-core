using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FocalProcessingFactors {

    public enum FocalProcessingFactorsSections {
        FocalProcessingFactorModelsSection,
        FocalProcessingFactorsSection
    }

    public sealed class FocalProcessingFactorsSummarizer : ActionResultsSummarizerBase<IFocalProcessingFactorsActionResult> {

        public override ActionType ActionType => ActionType.FocalProcessingFactors;

        public override void Summarize(
            ActionModuleConfig outputConfig,
            IFocalProcessingFactorsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<FocalProcessingFactorsSections>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            if (data.FocalProcessingFactors != null && outputSettings.ShouldSummarize(FocalProcessingFactorsSections.FocalProcessingFactorsSection)) {
                summarizeFocalProcessingFactors(data, subHeader, subOrder++);
            }
        }


        private void summarizeFocalProcessingFactors(ActionData data, SectionHeader header, int order) {
            var section = new ProcessingFactorDataSection() {
                SectionLabel = getSectionLabel(FocalProcessingFactorsSections.FocalProcessingFactorsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Focal processing factors",
                order++
            );
            section.Summarize(data.FocalProcessingFactors);
            subHeader.SaveSummarySection(section);
        }
    }
}
