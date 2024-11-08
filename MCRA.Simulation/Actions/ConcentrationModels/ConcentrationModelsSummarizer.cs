using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.ConcentrationModels {
    public enum ConcentrationModelsSections {
        ConcentrationModelGraphsSection,
        ConcentrationModelsTableSection,
        CumulativeConcentrationModelsTableSection,
        OccurrencePatternsSection
    }
    public sealed class ConcentrationModelsSummarizer : ActionResultsSummarizerBase<ConcentrationModelsActionResult> {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void Summarize(ActionModuleConfig sectionConfig, ConcentrationModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationModelsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new ConcentrationModelsSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.Units = collectUnits(data);
            subHeader.SaveSummarySection(section);

            var subOrder = 0;

            if (outputSettings.ShouldSummarize(ConcentrationModelsSections.ConcentrationModelsTableSection)
                && (actionResult.ConcentrationModels?.Count > 0)) {
                summarizeConcentrationModelsTableSection(actionResult, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(ConcentrationModelsSections.CumulativeConcentrationModelsTableSection)
                && (actionResult.CumulativeConcentrationModels?.Count > 0)) {
                summarizeCumulativeConcentrationModels(actionResult, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(ConcentrationModelsSections.ConcentrationModelGraphsSection)
                && ((actionResult.CumulativeConcentrationModels?.Count > 0)
                    || (actionResult.ConcentrationModels?.Count > 0))) {
                summarizeConcentrationModelCharts(actionResult, subHeader, subOrder++);
            }

            if (actionResult.SimulatedOccurrencePatterns != null && outputSettings.ShouldSummarize(ConcentrationModelsSections.OccurrencePatternsSection)) {
                summarizeSimulatedOccurrencePatterns(actionResult, subHeader, subOrder++);
            }
        }

        public void SummarizeUncertain(
            ActionModuleConfig sectionConfig,
            ConcentrationModelsActionResult actionResult,
            SectionHeader header
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationModelsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.GetSubSectionHeader<ConcentrationModelsSection>();
            if (subHeader != null) {
                summarizeConcentrationModelsUncertain(
                    actionResult.ConcentrationModels,
                    actionResult.CumulativeConcentrationModels,
                    subHeader
                );
            }
        }

        private void summarizeConcentrationModelsTableSection(
            ConcentrationModelsActionResult actionResult,
            SectionHeader subHeader,
            int subOrder
        ) {
            var concentrationModels = actionResult.ConcentrationModels;
            var concentrationModelRecords = concentrationModels
                .Where(c => c.Value.ModelType != ConcentrationModelType.Empirical || c.Value.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Key.Food, r.Key.Substance, r.Value, false, false);
                    return record;
                })
                .OrderBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (concentrationModelRecords?.Any(c => !c.CompoundName.StartsWith("_")) ?? false) {
                var subSection = new ConcentrationModelsTableSection {
                    ConcentrationModelRecords = concentrationModelRecords.Where(c => !c.CompoundName.StartsWith("_")).ToList(),
                    SectionLabel = getSectionLabel(ConcentrationModelsSections.ConcentrationModelsTableSection)
                };
                var subSubHeader = subHeader.AddSubSectionHeaderFor(
                    subSection,
                    "Concentration models table",
                    subOrder++
                );
                subSubHeader.SaveSummarySection(subSection);
            }
        }

        private void summarizeCumulativeConcentrationModels(
            ConcentrationModelsActionResult actionResult,
            SectionHeader header,
            int subOrder
        ) {
            var cumulativeConcentrationModels = actionResult.CumulativeConcentrationModels;
            var cumulativeConcentrationModelRecords = cumulativeConcentrationModels?
                .Where(c => c.Value.ModelType != ConcentrationModelType.Empirical || c.Value.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Key, r.Value.Compound, r.Value, true, false);
                    return record;
                })
                .ToList();
            if (cumulativeConcentrationModelRecords?.Count > 0) {
                var subSection = new ConcentrationModelsTableSection {
                    ConcentrationModelRecords = cumulativeConcentrationModelRecords,
                    SectionLabel = getSectionLabel(ConcentrationModelsSections.CumulativeConcentrationModelsTableSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    subSection,
                    "Reference substance equivalents models",
                    subOrder++
                );
                subHeader.SaveSummarySection(subSection);
            }
        }

        private void summarizeConcentrationModelCharts(
            ConcentrationModelsActionResult actionResult,
            SectionHeader header,
            int subOrder
        ) {
            var concentrationModels = actionResult.ConcentrationModels;
            var concentrationModelRecords = concentrationModels
                .Where(c => c.Value.ModelType != ConcentrationModelType.Empirical || c.Value.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Key.Food, r.Key.Substance, r.Value, false);
                    return record;
                })
                .OrderBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var cumulativeConcentrationModels = actionResult.CumulativeConcentrationModels;
            var cumulativeConcentrationModelRecords = cumulativeConcentrationModels?
                .Where(c => c.Value.ModelType != ConcentrationModelType.Empirical || c.Value.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Key, r.Value.Compound, r.Value, true);
                    return record;
                })
                .ToList();
            var allConcentrationModelRecords = cumulativeConcentrationModelRecords != null
                ? concentrationModelRecords.Concat(cumulativeConcentrationModelRecords).ToList()
                : concentrationModelRecords;

            if (allConcentrationModelRecords?.Count > 0) {
                var subSection = new ConcentrationModelsGraphSection {
                    ConcentrationModelRecords = allConcentrationModelRecords,
                    SectionLabel = getSectionLabel(ConcentrationModelsSections.ConcentrationModelGraphsSection)
                };
                var subHeader = header.AddSubSectionHeaderFor(
                    subSection,
                    "Concentration model graphs",
                    subOrder++
                );
                subHeader.SaveSummarySection(subSection);
            }
        }

        private void summarizeSimulatedOccurrencePatterns(
                ConcentrationModelsActionResult result,
                SectionHeader subHeader,
                int subOrder
            ) {
            var subSection = new OccurrencePatternMixtureSummarySection() {
                SectionLabel = getSectionLabel(ConcentrationModelsSections.OccurrencePatternsSection)
            };
            var sub2Header = subHeader.AddSubSectionHeaderFor(
                subSection,
                "Simulated occurrence patterns",
                subOrder++
            );
            subSection.Summarize(result.SimulatedOccurrencePatterns, false);
            sub2Header.SaveSummarySection(subSection);
        }

        private static List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName())
            };
            return result;
        }

        private void summarizeConcentrationModelsUncertain(
            IDictionary<(Food, Compound), ConcentrationModel> concentrationModels,
            IDictionary<Food, ConcentrationModel> cumulativeConcentrationModels,
            SectionHeader header
        ) {
            if (concentrationModels?.Count > 0) {
                var subHeader = header.GetSubSectionHeaderFromTitleString<ConcentrationModelsTableSection>("Concentration models table");
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ConcentrationModelsTableSection;
                    section.SummarizeUncertain(concentrationModels);
                    subHeader.SaveSummarySection(section);
                }
            }
            if (cumulativeConcentrationModels?.Count > 0) {
                var subHeader = header.GetSubSectionHeaderFromTitleString<ConcentrationModelsTableSection>("Reference substance equivalents models");
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ConcentrationModelsTableSection;
                    section.SummarizeUncertain(cumulativeConcentrationModels);
                    subHeader.SaveSummarySection(section);
                }
            }
        }
    }
}
