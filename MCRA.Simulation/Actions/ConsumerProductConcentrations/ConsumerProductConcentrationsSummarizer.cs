using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrations {
    public enum ConsumerProductConcentrationsSections {
        //No sub-sections
    }
    public class ConsumerProductConcentrationsSummarizer : ActionResultsSummarizerBase<IConsumerProductConcentrationsActionResult> {

        public override ActionType ActionType => ActionType.ConsumerProductConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumerProductConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            //var section = new DustConcentrationDistributionsSummarySection() {
            //    SectionLabel = ActionType.ToString()
            //};
            //var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            //subHeader.Units = collectUnits(data, sectionConfig);

            //section.Summarize(
            //    data.DustConcentrationDistributions,
            //    data.DustConcentrationUnit,
            //    sectionConfig.VariabilityLowerPercentage,
            //    sectionConfig.VariabilityUpperPercentage
            //);
            //subHeader.SaveSummarySection(section);
        }
        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}")
            };
            return result;
        }
    }
}
