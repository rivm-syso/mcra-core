using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.Populations {
    public enum PopulationsSections {
        PopulationsSummarySection,
        PopulationCharacteristicsSummarySection
    }
    public class PopulationsSummarizer : ActionResultsSummarizerBase<IPopulationsActionResult> {

        public override ActionType ActionType => ActionType.Populations;

        public override void Summarize(
            ActionModuleConfig sectionConfig,
            IPopulationsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<PopulationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());

            subHeader.Units = collectUnits(data);

            if (outputSettings.ShouldSummarize(PopulationsSections.PopulationsSummarySection)
                 && (data.SelectedPopulation != null)
            ) {
                summarizePopulation(
                    data,
                    subHeader,
                    order++
                );
            }

            if (outputSettings.ShouldSummarize(PopulationsSections.PopulationsSummarySection)
                 && data.SelectedPopulation != null
                 && data.SelectedPopulation.PopulationCharacteristics.Count > 0
            ) {
                summarizePopulationCharacteristics(
                    data,
                    subHeader,
                    order++
                );
            }
        }

        private void summarizePopulation(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new PopulationPropertySummarySection() {
                SectionLabel = getSectionLabel(PopulationsSections.PopulationsSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Population definition", order);
            section.Summarize(data.SelectedPopulation);
            subHeader.SaveSummarySection(section);
        }

        private void summarizePopulationCharacteristics(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new PopulationCharacteristicsSummarySection() {
                SectionLabel = getSectionLabel(PopulationsSections.PopulationCharacteristicsSummarySection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Population characteristics", order);
            section.Summarize(data.SelectedPopulation.PopulationCharacteristics);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}
