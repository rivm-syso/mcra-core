using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.HbmSingleValueExposures {
    public enum HbmSingleValueExposuresSections {
        PercentilesSection
    }
    public sealed class HbmSingleValueExposuresSummarizer : ActionModuleResultsSummarizer<HbmSingleValueExposuresModuleConfig, HbmSingleValueExposuresActionResult> {

        public HbmSingleValueExposuresSummarizer(HbmSingleValueExposuresModuleConfig config) : base(config) {
        }

        public override void Summarize(
            ActionModuleConfig outputConfig,
            HbmSingleValueExposuresActionResult result,
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var outputSettings = new ModuleOutputSectionsManager<HbmSingleValueExposuresSections>(outputConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            subHeader.Units = collectUnits(data);
            if (outputSettings.ShouldSummarize(HbmSingleValueExposuresSections.PercentilesSection)
                && (data.HbmSingleValueExposureSets?.Any() ?? false)
            ) {
                summarize(
                    data.HbmSingleValueExposureSets,
                    subHeader,
                    order++
                );
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(ActionData data) {
            var result = new List<ActionSummaryUnitRecord> {
                new("DoseUnit", data.SingleValueConcentrationUnit.GetShortDisplayName()),
                new("LowerPercentage", $"p{_configuration.VariabilityLowerPercentage}"),
                new("UpperPercentage", $"p{_configuration.VariabilityUpperPercentage}"),
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}"),
            };
            return result;
        }

        private void summarize(
           ICollection<HbmSingleValueExposureSet> hbmSingleValueExposureSets,
           SectionHeader header,
           int order
        ) {
            var section = new HbmSingleValueExposuresSummarySection() {
                SectionLabel = getSectionLabel(HbmSingleValueExposuresSections.PercentilesSection)
            };
            section.Summarize(
                hbmSingleValueExposureSets
            );
            var subHeader = header.AddSubSectionHeaderFor(
                section,
                "HBM study and percentiles",
                order
            );
            subHeader.SaveSummarySection(section);
        }
    }
}
