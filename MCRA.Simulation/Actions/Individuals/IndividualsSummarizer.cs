using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Individuals {
    public enum IndividualsSections {
        IndividualsSection
    }

    public sealed class IndividualsSummarizer : ActionModuleResultsSummarizer<IndividualsModuleConfig, IndividualsActionResult> {

        public IndividualsSummarizer(IndividualsModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IndividualsActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<IndividualsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            if (data.Individuals != null
               && outputSettings.ShouldSummarize(IndividualsSections.IndividualsSection)
            ) {

                // Main summary section
                var section = new IndividualsSummarySection() {
                    SectionLabel = ActionType.ToString()
                };
                section.Summarize(
                    data.Individuals,
                    data.SelectedPopulation,
                    _configuration.IsCompute
                );
                var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
                subHeader.SaveSummarySection(section);
            }
        }
    }
}