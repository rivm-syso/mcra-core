using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustConcentrationDistributions {
    public enum DustConcentrationDistributionsSections {
        DustConcentrationModelsSection,
        DustConcentrationModelGraphsSection
    }
    public class DustConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<DustConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.DustConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, DustConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DustConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            subHeader.Units = collectUnits(data, sectionConfig);

            if (outputSettings.ShouldSummarize(DustConcentrationDistributionsSections.DustConcentrationModelsSection)
               && data.DustConcentrationModels?.Count > 0
            ) {
                summarizeDustConcentrationModels(
                    data.DustConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }

            if (outputSettings.ShouldSummarize(DustConcentrationDistributionsSections.DustConcentrationModelGraphsSection)
              && (data.DustConcentrationModels?.Count > 0)) {
                summarizeDustConcentrationModelCharts(
                    data.DustConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }
        }

        private void summarizeDustConcentrationModels(
           IDictionary<Compound, ConcentrationModel> concentrationModels,
           SectionHeader header,
           int order
        ) {
            var section = new DustConcentrationModelsTableSection() {
                SectionLabel = getSectionLabel(DustConcentrationDistributionsSections.DustConcentrationModelsSection)
            };
            section.SummarizeTableRecords(concentrationModels);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Dust concentration models per substance",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDustConcentrationModelCharts(
           IDictionary<Compound, ConcentrationModel> concentrationModels,
           SectionHeader header,
           int subOrder
        ) {
            var section = new DustConcentrationModelsGraphSection {
                SectionLabel = getSectionLabel(DustConcentrationDistributionsSections.DustConcentrationModelGraphsSection)
            };
            section.SummarizeGraphRecords(concentrationModels, ExposureSource.Dust.GetShortDisplayName());
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Dust concentration model graphs",
                subOrder++
            );
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
                new("ConcentrationUnit", data.DustConcentrationDistributionUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}
