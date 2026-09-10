using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilExposureDeterminants {
    public enum SoilExposureDeterminantsSections {
        SoilIngestionsDataSection,
        SoilBioaccessibilityFractionsDataSection
    }

    public class SoilExposureDeterminantsSummarizer : ActionResultsSummarizerBase<ISoilExposureDeterminantsActionResult> {

        public override ActionType ActionType => ActionType.SoilExposureDeterminants;

        public override void Summarize(ActionModuleConfig sectionConfig, ISoilExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SoilExposureDeterminantsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            // Summarize soil ingestion data.
            if ( data.SoilIngestions != null && outputSettings.ShouldSummarize(SoilExposureDeterminantsSections.SoilIngestionsDataSection)) {
                summarizeSoilIngestions(data, subHeader, subOrder++);
            }

            if (data.SoilBioaccessibilityFractions != null && outputSettings.ShouldSummarize(SoilExposureDeterminantsSections.SoilBioaccessibilityFractionsDataSection)) {
                summarizeSoilBioaccessibilityFractions(data, subHeader, subOrder++);
            }
        }

        private void summarizeSoilIngestions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new SoilIngestionsDataSection() {
                SectionLabel = getSectionLabel(SoilExposureDeterminantsSections.SoilIngestionsDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Soil ingestions", order);
            section.Summarize(
                data.SoilIngestions
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeSoilBioaccessibilityFractions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new SoilBioaccessibilityFractionsDataSection() {
                SectionLabel = getSectionLabel(SoilExposureDeterminantsSections.SoilBioaccessibilityFractionsDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Soil bioaccessibility fractions", order);
            section.Summarize(
                data.SoilBioaccessibilityFractions
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

