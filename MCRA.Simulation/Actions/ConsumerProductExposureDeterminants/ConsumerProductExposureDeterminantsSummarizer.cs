using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConsumerProductExposureDeterminants {
    public enum ConsumerProductExposureDeterminantsSections {
        CPApplicationAmountsDataSection,
        CPExposureFractionsDataSection
    }

    public class ConsumerProductExposureDeterminantsSummarizer : ActionResultsSummarizerBase<IConsumerProductExposureDeterminantsActionResult> {

        public override ActionType ActionType => ActionType.ConsumerProductExposureDeterminants;

        public override void Summarize(ActionModuleConfig sectionConfig, IConsumerProductExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConsumerProductExposureDeterminantsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            subHeader.Units = collectUnits(data, sectionConfig);
            var subOrder = 0;

            //Summarize exposure fractions.
            if (data.ConsumerProductExposureFractions != null) {
                summarizeExposureFractions(data, subHeader, subOrder++);
            }

            // Summarize application amounts.
            if (data.ConsumerProductApplicationAmounts != null) {
                summarizeApplicationAmounts(data, subHeader, subOrder++);
            }
        }

        private void summarizeExposureFractions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ConsumerProductExposureFractionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Consumer product exposure fractions", order);
            section.Summarize(
                data.ConsumerProductExposureFractions
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeApplicationAmounts(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new ConsumerProductApplicationAmountsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Consumer product application amounts", order);
            section.Summarize(
                data.ConsumerProductApplicationAmounts
            );
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ApplicationAmountUnit", data.CPApplicationAmountUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}

