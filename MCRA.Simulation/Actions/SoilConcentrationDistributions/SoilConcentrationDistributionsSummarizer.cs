using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilConcentrationDistributions {
    public enum SoilConcentrationDistributionsSections {
        SoilConcentrationModelsSection,
        SoilConcentrationModelGraphsSection
    }
    public class SoilConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<SoilConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.SoilConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, SoilConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SoilConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            subHeader.Units = collectUnits(data, sectionConfig);

            if (outputSettings.ShouldSummarize(SoilConcentrationDistributionsSections.SoilConcentrationModelsSection)
               && data.SoilConcentrationModels?.Count > 0
            ) {
                summarizeSoilConcentrationModels(
                    data.SoilConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }

            if (outputSettings.ShouldSummarize(SoilConcentrationDistributionsSections.SoilConcentrationModelGraphsSection)
              && (data.SoilConcentrationModels?.Count > 0)) {
                summarizeSoilConcentrationModelCharts(
                    data.SoilConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }
        }

        private void summarizeSoilConcentrationModels(
           IDictionary<Compound, ConcentrationModel> concentrationModels,
           SectionHeader header,
           int order
        ) {
            var section = new SoilConcentrationModelsTableSection() {
                SectionLabel = getSectionLabel(SoilConcentrationDistributionsSections.SoilConcentrationModelsSection)
            };
            section.SummarizeTableRecords(concentrationModels);
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Soil concentration models per substance",
                order
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSoilConcentrationModelCharts(
           IDictionary<Compound, ConcentrationModel> concentrationModels,
           SectionHeader header,
           int subOrder
        ) {
            var section = new SoilConcentrationModelsGraphSection {
                SectionLabel = getSectionLabel(SoilConcentrationDistributionsSections.SoilConcentrationModelGraphsSection)
            };
            section.SummarizeGraphRecords(concentrationModels, ExposureSource.Soil.GetShortDisplayName());
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "Soil concentration model graphs",
                subOrder++
            );
            subHeader.SaveSummarySection(section);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
                new("ConcentrationUnit", data.SoilConcentrationDistributionUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}
