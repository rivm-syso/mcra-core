using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Actions.EnvironmentalBurdenOfDisease;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.DustExposureDeterminants {
    public enum DustExposureDeterminantsSections {
        DustIngestionsDataSection,
        DustBodyExposureFractionsDataSection,
        DustAdherenceAmountsDataSection,
        DustAvailabilityFractionsDataSection,
        DustBioaccessibilityFractionsDataSection,
    }

    public class DustExposureDeterminantsSummarizer : ActionResultsSummarizerBase<IDustExposureDeterminantsActionResult> {

        public override ActionType ActionType => ActionType.DustExposureDeterminants;

        public override void Summarize(ActionModuleConfig sectionConfig, IDustExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<DustExposureDeterminantsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            int subOrder = 0;

            // Summarize dust ingestion data.
            if (data.DustIngestions != null  && outputSettings.ShouldSummarize(DustExposureDeterminantsSections.DustIngestionsDataSection)) {
                summarizeDustIngestions(data, subHeader, subOrder++);
            }

            // Summarize dust body exposure fractions.
            if (data.DustBodyExposureFractions != null && outputSettings.ShouldSummarize(DustExposureDeterminantsSections.DustBodyExposureFractionsDataSection)) {
                summarizeDustBodyExposureFractions(data, subHeader, subOrder++);
            }

            // Summarize dust adherence fractions.
            if (data.DustAdherenceAmounts != null && outputSettings.ShouldSummarize(DustExposureDeterminantsSections.DustAdherenceAmountsDataSection)) {
                summarizeDustAdherenceAmounts(data, subHeader, subOrder++);
            }

            // Summarize dust availability fractions.
            if (data.DustAvailabilityFractions != null && outputSettings.ShouldSummarize(DustExposureDeterminantsSections.DustAvailabilityFractionsDataSection)) {
                summarizeDustAvailabilityFractions(data, subHeader, subOrder++);
            }
            // Summarize dust bioaccessibility fractions.
            if (data.DustBioaccessibilityFractions != null && outputSettings.ShouldSummarize(DustExposureDeterminantsSections.DustBioaccessibilityFractionsDataSection)) {
                summarizeDustBioaccessibilityFractions(data, subHeader, subOrder++);
            }
        }

        private void summarizeDustIngestions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustIngestionsDataSection() {
                SectionLabel = getSectionLabel(DustExposureDeterminantsSections.DustIngestionsDataSection)
            };
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
            var section = new DustBodyExposureFractionsDataSection() {
                SectionLabel = getSectionLabel(DustExposureDeterminantsSections.DustBodyExposureFractionsDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust body exposure fractions", order);
            section.Summarize(
                data.DustBodyExposureFractions,
                ExposureSource.Dust,
                "body exposure fraction"
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDustAdherenceAmounts(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustAdherenceAmountsDataSection() {
                SectionLabel = getSectionLabel(DustExposureDeterminantsSections.DustAdherenceAmountsDataSection)
            };
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
            var section = new DustAvailabilityFractionsDataSection() {
                SectionLabel = getSectionLabel(DustExposureDeterminantsSections.DustAvailabilityFractionsDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust availiability fractions", order);
            section.Summarize(
                data.DustAvailabilityFractions
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeDustBioaccessibilityFractions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new DustBioaccessibilityFractionsDataSection(){
                SectionLabel = getSectionLabel(DustExposureDeterminantsSections.DustBioaccessibilityFractionsDataSection)
            };
            var subHeader = header.AddSubSectionHeaderFor(section, "Dust bioaccessibility fractions", order);
            section.Summarize(
                data.DustBioaccessibilityFractions
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

