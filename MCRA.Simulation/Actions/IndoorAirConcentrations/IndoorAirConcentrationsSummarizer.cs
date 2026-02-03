using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.IndoorAirConcentrations {
    public enum IndoorAirConcentrationsSections {
        ConcentrationsSection,
        PercentilesSection
    }
    public class IndoorAirConcentrationsSummarizer(IndoorAirConcentrationsModuleConfig config)
           : ActionModuleResultsSummarizer<IndoorAirConcentrationsModuleConfig, IIndoorAirConcentrationsActionResult>(config) {

        public override ActionType ActionType => ActionType.IndoorAirConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IIndoorAirConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<IndoorAirConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new IndoorAirConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);
            if (outputSettings.ShouldSummarize(IndoorAirConcentrationsSections.ConcentrationsSection)) {
                section.Summarize(
                    data.IndoorAirConcentrations,
                    data.IndoorAirConcentrationUnit,
                    sectionConfig.VariabilityLowerPercentage,
                    sectionConfig.VariabilityUpperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
            summarizePercentiles(data, sectionConfig, subHeader, ++order);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{sectionConfig.UncertaintyLowerBound}"),
                new("UpperBound", $"p{sectionConfig.UncertaintyUpperBound}"),
                new("ConcentrationUnit", $"{data.IndoorAirConcentrationUnit.GetShortDisplayName()}")
            };
            return result;
        }

        private void summarizePercentiles(
            ActionData data,
            ActionModuleConfig sectionConfig,
            SectionHeader header,
            int order
        ) {
            var section = new IndoorAirConcentrationPercentilesSection() {
                SectionLabel = getSectionLabel(IndoorAirConcentrationsSections.PercentilesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", order++);
            section.Summarize(
                [.. data.IndoorAirConcentrations.Select(SimpleSubstanceConcentration.Clone)],
                data.ActiveSubstances,
                sectionConfig.UncertaintyLowerBound,
                sectionConfig.UncertaintyUpperBound,
                sectionConfig.SelectedPercentiles
            );
            subHeader.SaveSummarySection(section);
        }
        public void SummarizeUncertain(
                ActionData data,
                IIndoorAirConcentrationsActionResult actionResult,
                SectionHeader header
            ) {
            var subHeader = header.GetSubSectionHeader<IndoorAirConcentrationPercentilesSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as IndoorAirConcentrationPercentilesSection;
                section.SummarizeUncertainty(
                    [.. data.IndoorAirConcentrations.Select(SimpleSubstanceConcentration.Clone)],
                    data.ActiveSubstances,
                    _configuration.SelectedPercentiles
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
