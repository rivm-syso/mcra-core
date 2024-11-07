using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualStatisticsSummarySectionView : SectionView<HbmIndividualStatisticsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            {
                sb.AppendDescriptionTable(
                    "HumanMonitoringSurveySummaryTable",
                    Model.SectionId,
                    Model.individualsSummaryRecord,
                    ViewBag,
                    header: false
                );
            }
            if (Model.SelectedPropertyRecords?.Any() ?? false) {
                sb.AppendTable(
                    Model,
                    Model.SelectedPropertyRecords,
                    "HumanMonitoringSelectedPropertiesTable",
                    ViewBag,
                    caption: "Selected population properties and levels.",
                    saveCsv: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No population properties selected (full population)");
            }

            if (Model.HbmPopulationRecords?.Any() ?? false) {
                var hiddenProperties = new List<string>();
                if (Model.HbmPopulationRecords.All(r => r.Min == null)) {
                    hiddenProperties.Add("Min");
                }
                if (Model.HbmPopulationRecords.All(r => r.Max == null)) {
                    hiddenProperties.Add("Max");
                }
                sb.AppendTable(
                    Model,
                    Model.HbmPopulationRecords,
                    "HumanMonitoringPopulationCharacteristicsDataTable",
                    ViewBag,
                    caption: "Human biomonitoring individuals statistics.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
