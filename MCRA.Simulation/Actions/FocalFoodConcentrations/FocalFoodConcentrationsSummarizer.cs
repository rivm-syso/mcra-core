using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Actions.FocalFoodConcentrations {
    public enum FocalFoodConcentrationsSections {
        SamplesByFoodAndSubstanceSection
    }
    public sealed class FocalFoodConcentrationsSummarizer : ActionResultsSummarizerBase<IFocalFoodConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.FocalFoodConcentrations;

        public override void Summarize(ProjectDto project, IFocalFoodConcentrationsActionResult result, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<FocalFoodConcentrationsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var section = new ConcentrationDataSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            section.Summarize(data.FocalCommoditySubstanceSampleCollections, data.AllCompounds);
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(project, data);

            if ((data.FocalCommoditySubstanceSampleCollections?.Any(r => r.SampleCompoundRecords.Any()) ?? false)
               && outputSettings.ShouldSummarize(FocalFoodConcentrationsSections.SamplesByFoodAndSubstanceSection)
            ) {
                summarizeSamplesByFoodAndSubstance(project, data, subHeader, order++);
            }
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ProjectDto project, ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()),
                new ActionSummaryUnitRecord("LowerPercentage", $"p{project.OutputDetailSettings.LowerPercentage}"),
                new ActionSummaryUnitRecord("UpperPercentage", $"p{project.OutputDetailSettings.UpperPercentage}")
            };
            return result;
        }

        private void summarizeSamplesByFoodAndSubstance(ProjectDto project, ActionData data, SectionHeader header, int order) {
            var section = new SamplesByFoodSubstanceSection() {
                SectionLabel = getSectionLabel(FocalFoodConcentrationsSections.SamplesByFoodAndSubstanceSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Samples by food and substance",
                order
            );
            section.Summarize(
                data.FocalCommoditySubstanceSampleCollections,
                null,
                project.OutputDetailSettings.LowerPercentage,
                project.OutputDetailSettings.UpperPercentage
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
