using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;

namespace MCRA.Simulation.Actions.ConcentrationLimits {
    public enum ConcentrationLimitsSections {
        //No sub-sections
    }
    public class ConcentrationLimitsSummarizer : ActionResultsSummarizerBase<IConcentrationLimitsActionResult> {

        public override ActionType ActionType => ActionType.ConcentrationLimits;

        public override void Summarize(ProjectDto project, IConcentrationLimitsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationLimitsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new ConcentrationLimitsDataSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            section.Summarize(data.MaximumConcentrationLimits.Values, data.ConcentrationUnit);
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord>();
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()));
            return result;
        }
    }
}
