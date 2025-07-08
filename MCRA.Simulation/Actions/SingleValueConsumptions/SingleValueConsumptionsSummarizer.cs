using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {
    public enum SingleValueConsumptionsSections {
        SingleValueConsumptionsSection,
        SingleValueConsumptionsDataSection,
    }
    public sealed class SingleValueConsumptionsSummarizer : ActionResultsSummarizerBase<SingleValueConsumptionsActionResult> {

        public override ActionType ActionType => ActionType.SingleValueConsumptions;

        public override void Summarize(ActionModuleConfig sectionConfig, SingleValueConsumptionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueConsumptionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString()); subHeader.Units = collectUnits(data);
            var subOrder = 0;

            if (data.SingleValueConsumptionModels != null && outputSettings.ShouldSummarize(SingleValueConsumptionsSections.SingleValueConsumptionsSection)) {
                summarizeConsumptionsByFoodAsMeasured(data, subHeader, subOrder++);
            }
            if (data.FoodConsumptionSingleValues != null && outputSettings.ShouldSummarize(SingleValueConsumptionsSections.SingleValueConsumptionsDataSection)) {
                summarizeConsumptionSingleValues(data, subHeader, subOrder++);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ConsumptionIntakeUnit", data.SingleValueConsumptionIntakeUnit.GetShortDisplayName()),
                new("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()),
            };
            return result;
        }

        private void summarizeConsumptionsByFoodAsMeasured(ActionData data, SectionHeader header, int order) {
            var section = new SingleValueConsumptionSummarySection() {
                SectionLabel = getSectionLabel(SingleValueConsumptionsSections.SingleValueConsumptionsSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Single value consumptions",
                order
            );
            section.Summarize(data.SingleValueConsumptionModels);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeConsumptionSingleValues(ActionData data, SectionHeader header, int order) {
            var section = new SingleValueConsumptionsDataSummarySection() {
                SectionLabel = getSectionLabel(SingleValueConsumptionsSections.SingleValueConsumptionsDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Single value consumption data",
                order
            );
            section.Summarize(
                data.FoodConsumptionSingleValues
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
