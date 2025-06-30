using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrations {
    public enum ConsumerProductConcentrationsSections {
        ConsumerProductConcentrationsSection
    }
    public sealed class ConsumerProductConcentrationsSummarizer : ActionModuleResultsSummarizer<ConsumerProductConcentrationsModuleConfig, IConsumerProductConcentrationsActionResult> {
        public ConsumerProductConcentrationsSummarizer(ConsumerProductConcentrationsModuleConfig config) : base(config) {
        }
        public override ActionType ActionType => ActionType.ConsumerProductConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumerProductConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            subHeader.Units = collectUnits(data, sectionConfig);

            if (outputSettings.ShouldSummarize(ConsumerProductConcentrationsSections.ConsumerProductConcentrationsSection)) {
                summarizeConsumerProductConcentration(
                    data.AllConsumerProductConcentrations,
                    data.ConsumerProductConcentrationUnit,
                    subHeader,
                    subOrder++
                );
            }
        }
        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
                new("ConcentrationUnit", data.ConsumerProductConcentrationUnit.GetShortDisplayName())
            };
            return result;
        }

        private void summarizeConsumerProductConcentration(
           ICollection<ConsumerProductConcentration> consumerProductConcentrations,
           ConcentrationUnit consumerProductConcentrationUnit,
           SectionHeader header,
           int order
       ) {
            var section = new ConsumerProductConcentrationsSection() {
                SectionLabel = getSectionLabel(ConsumerProductConcentrationsSections.ConsumerProductConcentrationsSection)
            };
            section.Summarize(
                consumerProductConcentrations,
                consumerProductConcentrationUnit,
                _configuration.VariabilityLowerPercentage,
                _configuration.VariabilityUpperPercentage
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Concentrations per substance and consumer product",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
