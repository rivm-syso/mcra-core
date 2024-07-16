using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.DietaryExposures;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustExposureDeterminants {
    public enum DustExposureDeterminantsSections {
        DustIngestionsDataSection,
        DustBodyExposureFractionsDataSection
    }

    public class DustExposureDeterminantsSummarizer : ActionResultsSummarizerBase<IDustExposureDeterminantsActionResult> {

        public override ActionType ActionType => ActionType.DustExposureDeterminants;

        public override void Summarize(ActionModuleConfig sectionConfig, IDustExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DustExposureDeterminantsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var section = new DustExposureDeterminantsDataSection() {
                SectionLabel = ActionType.ToString()
            };
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            subHeader.SaveSummarySection(section);

            int subOrder = 0;

            // Summarize dust ingestion data.
            if (data.DustIngestions != null) {
                summarizeDustIngestions(data, subHeader, subOrder++);
            }

            // Summarize dust body exposure fractions.
            if (data.DustBodyExposureFractions != null) {
                summarizeDustBodyExposureFractions(data, subHeader, subOrder++);
            }

            // Summarize dust adherence fractions.
            if (data.DustAdherenceAmounts != null) {
                summarizeDustAdherenceAmounts(data, subHeader, subOrder++);
            }

            // Summarize dust availability fractions.
            if (data.DustAvailabilityFractions != null) {
                summarizeDustAvailabilityFractions(data, subHeader, subOrder++);
            }
        }

        private void summarizeDustIngestions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustIngestionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust ingestions", order);
            section.Summarize(
                data.DustIngestions
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDustBodyExposureFractions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustBodyExposureFractionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust body exposure fractions", order);
            section.Summarize(
                data.DustBodyExposureFractions
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDustAdherenceAmounts(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustAdherenceAmountsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust adherence amounts", order);
            section.Summarize(
                data.DustAdherenceAmounts
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDustAvailabilityFractions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustAvailabilityFractionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust availiability fractions", order);
            section.Summarize(
                data.DustAvailabilityFractions
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

