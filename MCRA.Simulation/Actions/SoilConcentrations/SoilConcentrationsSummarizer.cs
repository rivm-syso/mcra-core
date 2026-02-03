using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilConcentrations {
    public enum SoilConcentrationsSections {
        ConcentrationsSection,
        PercentilesSection
    }
    public sealed class SoilConcentrationsSummarizer(SoilConcentrationsModuleConfig config)
        : ActionModuleResultsSummarizer<SoilConcentrationsModuleConfig, ISoilConcentrationsActionResult>(config) {

        public override ActionType ActionType => ActionType.SoilConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, ISoilConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SoilConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new SoilConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);
            if (outputSettings.ShouldSummarize(SoilConcentrationsSections.ConcentrationsSection)) {
                section.Summarize(
                data.SoilConcentrations,
                data.SoilConcentrationUnit,
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
                new("ConcentrationUnit", $"{data.SoilConcentrationUnit.GetShortDisplayName()}")
            };
            return result;
        }

        private void summarizePercentiles(
            ActionData data,
            ActionModuleConfig sectionConfig,
            SectionHeader header,
            int order
        ) {
            var section = new SoilConcentrationPercentilesSection() {
                SectionLabel = getSectionLabel(SoilConcentrationsSections.PercentilesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", order++);
            section.Summarize(
                [.. data.SoilConcentrations.Select(SimpleSubstanceConcentration.Clone)],
                data.ActiveSubstances,
                sectionConfig.UncertaintyLowerBound,
                sectionConfig.UncertaintyUpperBound,
                sectionConfig.SelectedPercentiles
            );
            subHeader.SaveSummarySection(section);
        }


        public void SummarizeUncertain(
            ActionData data,
            ISoilConcentrationsActionResult actionResult,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<SoilConcentrationPercentilesSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as SoilConcentrationPercentilesSection;
                section.SummarizeUncertainty(
                    [.. data.SoilConcentrations.Select(SimpleSubstanceConcentration.Clone)],
                    data.ActiveSubstances,
                    _configuration.SelectedPercentiles
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
