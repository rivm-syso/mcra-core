using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueConsumptions {
    public enum SingleValueConsumptionsSections {
        SingleValueConsumptionsSection,
        SingleValueConsumptionsDataSection,
    }
    public sealed class SingleValueConsumptionsSummarizer : ActionResultsSummarizerBase<SingleValueConsumptionsActionResult> {

        public override ActionType ActionType => ActionType.SingleValueConsumptions;

        public override void Summarize(ProjectDto project, SingleValueConsumptionsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueConsumptionsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var consumptionInputSection = new ConsumptionsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(consumptionInputSection, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(consumptionInputSection);

            var subOrder = 0;
            if (data.SingleValueConsumptionModels != null && outputSettings.ShouldSummarize(SingleValueConsumptionsSections.SingleValueConsumptionsSection)) {
                summarizeConsumptionsByFoodAsMeasured(project, data, subHeader, subOrder++);
            }
            if (data.FoodConsumptionSingleValues != null && outputSettings.ShouldSummarize(SingleValueConsumptionsSections.SingleValueConsumptionsDataSection)) {
                summarizeConsumptionSingleValues(data, subHeader, subOrder++);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ConsumptionIntakeUnit", data.SingleValueConsumptionIntakeUnit.GetShortDisplayName()),
                new ActionSummaryUnitRecord("BodyWeightUnit", data.BodyWeightUnit.GetShortDisplayName()),
            };
            return result;
        }

        private void summarizeConsumptionsByFoodAsMeasured(ProjectDto project, ActionData data, SectionHeader header, int order) {
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
