using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Generic;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustConcentrations {
    public enum DustConcentrationsSections {
        ConcentrationsSection,
        PercentilesSection
    }
    public sealed class DustConcentrationsSummarizer(DustConcentrationsModuleConfig config)
        : ActionModuleResultsSummarizer<DustConcentrationsModuleConfig, IDustConcentrationsActionResult>(config) {

        public override ActionType ActionType => ActionType.DustConcentrations;

        public override void Summarize(ActionModuleConfig sectionConfig, IDustConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DustConcentrationsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new DustConcentrationsSummarySection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data, sectionConfig);
            if (outputSettings.ShouldSummarize(DustConcentrationsSections.ConcentrationsSection)) {
                    section.Summarize(
                    data.DustConcentrations,
                    data.DustConcentrationUnit,
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
                new("ConcentrationUnit", $"{data.DustConcentrationUnit.GetShortDisplayName()}")
            };
            return result;
        }
        private void summarizePercentiles(
            ActionData data,
            ActionModuleConfig sectionConfig,
            SectionHeader header,
            int order
        ) {
            var section = new DustConcentrationPercentilesSection() {
                SectionLabel = getSectionLabel(DustConcentrationsSections.PercentilesSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", order++);
            section.Summarize(
                [.. data.DustConcentrations.Select(SimpleSubstanceConcentration.Clone)],
                data.ActiveSubstances,
                sectionConfig.UncertaintyLowerBound,
                sectionConfig.UncertaintyUpperBound,
                sectionConfig.SelectedPercentiles
            );
            subHeader.SaveSummarySection(section);
        }


        public void SummarizeUncertain(
            ActionData data,
            IDustConcentrationsActionResult actionResult,
            SectionHeader header
        ) {
            var subHeader = header.GetSubSectionHeader<DustConcentrationPercentilesSection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as DustConcentrationPercentilesSection;
                section.SummarizeUncertainty(
                    [.. data.DustConcentrations.Select(SimpleSubstanceConcentration.Clone)],
                    data.ActiveSubstances,
                    _configuration.SelectedPercentiles
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
