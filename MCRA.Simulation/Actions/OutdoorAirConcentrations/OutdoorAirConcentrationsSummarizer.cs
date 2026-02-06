using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.OutdoorAirConcentrations {
    public enum OutdoorAirConcentrationsSections {
        ConcentrationsSection,
        PercentilesSection
    }
    public class OutdoorAirConcentrationsSummarizer(OutdoorAirConcentrationsModuleConfig config)
        : ActionModuleResultsSummarizer<OutdoorAirConcentrationsModuleConfig, IOutdoorAirConcentrationsActionResult>(config) {

        public override ActionType ActionType => ActionType.OutdoorAirConcentrations;

        public override void Summarize(
            ActionModuleConfig sectionConfig, 
            IOutdoorAirConcentrationsActionResult actionResult, 
            ActionData data, 
            SectionHeader header, 
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<OutdoorAirConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new OutdoorAirConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);
            if (outputSettings.ShouldSummarize(OutdoorAirConcentrationsSections.ConcentrationsSection)) {
                section.Summarize(
                    data.OutdoorAirConcentrations,
                    data.OutdoorAirConcentrationsUnit.GetShortDisplayName(),
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
            var section = new OutdoorAirConcentrationPercentilesSection() {
                SectionLabel = getSectionLabel(OutdoorAirConcentrationsSections.PercentilesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", order++);
            section.Summarize(
                data.OutdoorAirConcentrations,
                data.ActiveSubstances,
                sectionConfig.UncertaintyLowerBound,
                sectionConfig.UncertaintyUpperBound,
                sectionConfig.SelectedPercentiles
            );
            subHeader.SaveSummarySection(section);
        }


        public void SummarizeUncertain(
            ActionData data,
            IOutdoorAirConcentrationsActionResult actionResult,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<OutdoorAirConcentrationPercentilesSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as OutdoorAirConcentrationPercentilesSection;
                section.SummarizeUncertainty(
                    data.OutdoorAirConcentrations,
                    data.ActiveSubstances,
                    _configuration.SelectedPercentiles
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
