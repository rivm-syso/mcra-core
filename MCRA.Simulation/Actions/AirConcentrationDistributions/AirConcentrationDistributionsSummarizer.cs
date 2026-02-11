using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.AirConcentrationDistributions {
    public enum AirConcentrationDistributionsSections {
        IndoorConcentrationModelsSection,
        OutdoorConcentrationModelsSection,
        IndoorConcentrationModelGraphsSection,
        OutdoorConcentrationModelGraphsSection
    }
    public class AirConcentrationDistributionsSummarizer : ActionResultsSummarizerBase<AirConcentrationDistributionsActionResult> {

        public override ActionType ActionType => ActionType.AirConcentrationDistributions;

        public override void Summarize(ActionModuleConfig sectionConfig, AirConcentrationDistributionsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<AirConcentrationDistributionsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;
            subHeader.Units = collectUnits(data, sectionConfig);

            if (outputSettings.ShouldSummarize(AirConcentrationDistributionsSections.IndoorConcentrationModelsSection)
               && data.IndoorAirConcentrationModels?.Count > 0
            ) {
                summarizeIndoorConcentrationModels(
                    data.IndoorAirConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }

            if (outputSettings.ShouldSummarize(AirConcentrationDistributionsSections.OutdoorConcentrationModelsSection)
               && data.OutdoorAirConcentrationModels?.Count > 0
            ) {
                summarizeOutdoorConcentrationModels(
                    data.OutdoorAirConcentrationModels,
                    subHeader,
                    subOrder++
                );
            }
   
        }

        private void summarizeIndoorConcentrationModels(
           IDictionary<Compound, ConcentrationModel> concentrationModels,
           SectionHeader header,
           int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader(
                "Indoor",
                order,
                AirConcentrationDistributionsSections.IndoorConcentrationModelsSection.ToString()
            );
            {
                var section = new IndoorAirConcentrationModelsTableSection() {
                    SectionLabel = getSectionLabel(AirConcentrationDistributionsSections.IndoorConcentrationModelsSection)
                };
                section.SummarizeTableRecords(concentrationModels);
                var sub2Header = subHeader.AddSubSectionHeaderFor(
                    section,
                    "Air concentration models per substance",
                    order++
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var section = new IndoorAirConcentrationModelsGraphSection {
                    SectionLabel = getSectionLabel(AirConcentrationDistributionsSections.IndoorConcentrationModelGraphsSection)
                };
                section.SummarizeGraphRecords(concentrationModels, ExposureSource.Air.GetShortDisplayName());
                var sub2Header = subHeader.AddSubSectionHeaderFor(
                    section,
                    "Air concentration model graphs",
                    order++
                );
                sub2Header.SaveSummarySection(section);
            }
        }

        private void summarizeOutdoorConcentrationModels(
           IDictionary<Compound, ConcentrationModel> concentrationModels,
           SectionHeader header,
           int order
        ) {
            var subHeader = header.AddEmptySubSectionHeader(
                "Outdoor",
                order,
                AirConcentrationDistributionsSections.IndoorConcentrationModelsSection.ToString()
            );
            {
                var section = new IndoorAirConcentrationModelsTableSection() {
                    SectionLabel = getSectionLabel(AirConcentrationDistributionsSections.OutdoorConcentrationModelsSection)
                };
                section.SummarizeTableRecords(concentrationModels);
                var sub2Header = subHeader.AddSubSectionHeaderFor(
                    section,
                    "Air concentration models per substance",
                    order++
                );
                sub2Header.SaveSummarySection(section);
            }
            {
                var section = new OutdoorAirConcentrationModelsGraphSection {
                    SectionLabel = getSectionLabel(AirConcentrationDistributionsSections.OutdoorConcentrationModelGraphsSection)
                };
                section.SummarizeGraphRecords(concentrationModels, ExposureSource.Air.GetShortDisplayName());
                var sub2Header = subHeader.AddSubSectionHeaderFor(
                    section,
                    "Air concentration model graphs",
                    order++
                );
                sub2Header.SaveSummarySection(section);
            }
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data, ActionModuleConfig sectionConfig) {
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerPercentage", $"p{sectionConfig.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{sectionConfig.VariabilityUpperPercentage}"),
                new("ConcentrationUnit", data.AirConcentrationDistributionUnit.GetShortDisplayName()),
            };
            return result;
        }
    }
}
