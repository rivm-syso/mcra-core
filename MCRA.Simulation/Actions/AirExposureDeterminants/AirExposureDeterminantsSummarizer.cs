using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Actions.AirExposureDeterminants {
    public enum AirExposureDeterminantsSections {
        AirVentilatoryFlowRatesDataSection,
        AirIndoorFractionsDataSection,
        AirBodyExposureFractionsDataSection,
    }

    public class AirExposureDeterminantsSummarizer : ActionResultsSummarizerBase<IAirExposureDeterminantsActionResult> {

        public override ActionType ActionType => ActionType.AirExposureDeterminants;

        public override void Summarize(ActionModuleConfig sectionConfig, IAirExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<AirExposureDeterminantsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }

            var subHeader = header.AddEmptySubSectionHeader(ActionType.GetDisplayName(), order, ActionType.ToString());
            var subOrder = 0;

            // Summarize air indoor fractions.
            if (data.AirIndoorFractions != null) {
                summarizeAirIndoorFractions(data, subHeader, subOrder++);
            }

            // Summarize air ventilatory flow rates fractions.
            if (data.AirVentilatoryFlowRates != null) {
                summarizeAirVentilatoryFlowRates(data, subHeader, subOrder++);
            }

            // Summarize air body exposure fractions.
            if (data.AirBodyExposureFractions != null) {
                summarizeAirBodyExposureFractions(data, subHeader, subOrder++);
            }
        }

        private void summarizeAirIndoorFractions(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new AirIndoorFractionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Air indoor fractions", order);
            section.Summarize(
                data.AirIndoorFractions
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeAirVentilatoryFlowRates(
            ActionData data,
            SectionHeader header,
            int order
        ) {
            var section = new AirVentilatoryFlowRatesDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Air ventilatory flow rates", order);
            section.Summarize(
                data.AirVentilatoryFlowRates,
                ExposureSource.Air,
                "ventilatory flow rate"
            );
            subHeader.SaveSummarySection(section);
        }

        private void summarizeAirBodyExposureFractions(
           ActionData data,
           SectionHeader header,
           int order
       ) {
            var section = new AirBodyExposureFractionsDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Air body exposure fractions", order);
            section.Summarize(
                data.AirBodyExposureFractions,
                ExposureSource.Air,
                "body exposure fraction"
            );
            subHeader.SaveSummarySection(section);
        }
    }
}

