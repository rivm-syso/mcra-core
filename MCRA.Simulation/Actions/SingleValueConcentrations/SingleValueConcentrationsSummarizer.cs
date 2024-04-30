using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.SingleValueConcentrations {
    public enum SingleValueConcentrationsSections {         
        //No sub-sections
    }
    public sealed class SingleValueConcentrationsSummarizer : ActionResultsSummarizerBase<SingleValueConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.SingleValueConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, SingleValueConcentrationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SingleValueConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            summarizeActiveSubstanceSingleValueConcentrations(data, subHeader, subOrder++);
            if (data.MeasuredSubstanceSingleValueConcentrations != data.ActiveSubstanceSingleValueConcentrations) {
                summarizeMeasuredSubstanceSingleValueConcentrations(data, subHeader, subOrder++);
            }
        }

        private void summarizeActiveSubstanceSingleValueConcentrations(ActionData data, SectionHeader header, int order) {
            var section = new SingleValueConcentrationsSummarySection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Single value concentrations", order);
            subHeader.Units = collectUnits(data);
            section.Summarize(data.ActiveSubstanceSingleValueConcentrations);
            subHeader.SaveSummarySection(section);
        }

        private void summarizeMeasuredSubstanceSingleValueConcentrations(ActionData data, SectionHeader header, int order) {
            var section = new SingleValueConcentrationsSummarySection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Measured single value concentrations", order);
            subHeader.Units = collectUnits(data);
            section.Summarize(data.MeasuredSubstanceSingleValueConcentrations);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ConcentrationUnit", data.SingleValueConcentrationUnit.GetShortDisplayName())
            };
            return result;
        }
    }
}
