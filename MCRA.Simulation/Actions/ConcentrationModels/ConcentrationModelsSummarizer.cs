using MCRA.Utils.ExtensionMethods;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Actions.ConcentrationModels {
    public enum ConcentrationModelsSections {
        ConcentrationModelGraphsSection,
        ConcentrationModelsTableSection,
        CumulativeConcentrationModelsTableSection,
        OccurrencePatternsSection
    }
    public sealed class ConcentrationModelsSummarizer : ActionResultsSummarizerBase<ConcentrationModelsActionResult> {

        public override ActionType ActionType => ActionType.ConcentrationModels;

        public override void Summarize(ProjectDto project, ConcentrationModelsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationModelsSections>(project, ActionType);
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
                && (actionResult.ConcentrationModels?.Any() ?? false)) {
                summarizeConcentrationModelsTableSection(actionResult, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(ConcentrationModelsSections.CumulativeConcentrationModelsTableSection)
                && (actionResult.CumulativeConcentrationModels?.Any() ?? false)) {
                summarizeCumulativeConcentrationModels(actionResult, subHeader, subOrder++);
            }

            if (outputSettings.ShouldSummarize(ConcentrationModelsSections.ConcentrationModelGraphsSection)
                && ((actionResult.CumulativeConcentrationModels?.Any() ?? false)
                    || (actionResult.ConcentrationModels?.Any() ?? false))) {
                summarizeConcentrationModelCharts(actionResult, subHeader, subOrder++);
            }

            if (actionResult.SimulatedOccurrencePatterns != null && outputSettings.ShouldSummarize(ConcentrationModelsSections.OccurrencePatternsSection)) {
                summarizeSimulatedOccurrencePatterns(actionResult, subHeader, subOrder++);
            }
        }

        public void SummarizeUncertain(
            ProjectDto project,
            ConcentrationModelsActionResult actionResult,
            SectionHeader header
        ) {
            var outputSettings = new ModuleOutputSectionsManager<ConcentrationModelsSections>(project, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.GetSubSectionHeader<ConcentrationModelsSection>();
            if (subHeader != null) {
                summarizeConcentrationModelsUncertain(
                    actionResult.ConcentrationModels.Values,
                    actionResult.CumulativeConcentrationModels?.Values,
                    subHeader
                );
            }
        }

        private void summarizeConcentrationModelsTableSection(
            ConcentrationModelsActionResult actionResult,
            SectionHeader subHeader,
            int subOrder
        ) {
            var concentrationModels = actionResult.ConcentrationModels.Values;
            var concentrationModelRecords = concentrationModels
                .Where(c => c.ModelType != ConcentrationModelType.Empirical || c.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Food, r.Compound, r, false, false);
                    return record;
                })
                .OrderBy(r => r.CompoundName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodName, System.StringComparer.OrdinalIgnoreCase)
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
            var cumulativeConcentrationModels = actionResult.CumulativeConcentrationModels?.Values;
            var cumulativeConcentrationModelRecords = cumulativeConcentrationModels?
                .Where(c => c.ModelType != ConcentrationModelType.Empirical || c.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Food, r.Compound, r, true, false);
                    return record;
                })
                .ToList();
            if (cumulativeConcentrationModelRecords?.Any() ?? false) {
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
            var concentrationModels = actionResult.ConcentrationModels.Values;
            var concentrationModelRecords = concentrationModels
                .Where(c => c.ModelType != ConcentrationModelType.Empirical || c.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Food, r.Compound, r, false);
                    return record;
                })
                .OrderBy(r => r.CompoundName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
            var cumulativeConcentrationModels = actionResult.CumulativeConcentrationModels?.Values;
            var cumulativeConcentrationModelRecords = cumulativeConcentrationModels?
                .Where(c => c.ModelType != ConcentrationModelType.Empirical || c.Residues.NumberOfResidues > 0)
                .AsParallel()
                .Select(r => {
                    var record = new ConcentrationModelRecord();
                    record.Summarize(r.Food, r.Compound, r, true);
                    return record;
                })
                .ToList();
            var allConcentrationModelRecords = cumulativeConcentrationModelRecords != null
                ? concentrationModelRecords.Concat(cumulativeConcentrationModelRecords).ToList()
                : concentrationModelRecords;

            if (allConcentrationModelRecords?.Any() ?? false) {
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
            var result = new List<ActionSummaryUnitRecord>();
            result.Add(new ActionSummaryUnitRecord("ConcentrationUnit", data.ConcentrationUnit.GetShortDisplayName()));
            return result;
        }

        private void summarizeConcentrationModelsUncertain(
            ICollection<ConcentrationModel> concentrationModels,
            ICollection<ConcentrationModel> cumulativeConcentrationModels,
            SectionHeader header
        ) {
            if (concentrationModels?.Any() ?? false) {
                var subHeader = header.GetSubSectionHeaderFromTitleString<ConcentrationModelsTableSection>("Concentration models table");
                if (subHeader != null) {
                    var section = subHeader.GetSummarySection() as ConcentrationModelsTableSection;
                    section.SummarizeUncertain(concentrationModels);
                    subHeader.SaveSummarySection(section);
                }
            }
            if (cumulativeConcentrationModels?.Any() ?? false) {
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
