using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.SoilExposureDeterminants {
    public enum SoilExposureDeterminantsSections {
        SoilIngestionsDataSection
    }

    public class SoilExposureDeterminantsSummarizer : ActionResultsSummarizerBase<ISoilExposureDeterminantsActionResult> {

        public override ActionType ActionType => ActionType.SoilExposureDeterminants;

        public override void Summarize(ActionModuleConfig sectionConfig, ISoilExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<SoilExposureDeterminantsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new SoilExposureDeterminantsDataSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.SaveSummarySection(section);

            int subOrder = 0;

            // Summarize soil ingestion data.
            if (data.SoilIngestions != null) {
                summarizeSoilIngestions(data, subHeader, subOrder++);
            }
        }

        private void summarizeSoilIngestions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new SoilIngestionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Soil ingestions", order);
            section.Summarize(
                data.SoilIngestions
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

