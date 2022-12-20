using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmSurveySummarySectionView : SectionView<HbmSurveySummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            hiddenProperties.Add("Code");
            //Render HTML
            sb.AppendTable(
                Model,
                Model.Records,
                "HumanMonitoringSurveySummaryTable",
                ViewBag,
                caption: "Human monitoring survey.",
                saveCsv: true,
                header: true
            );

            if ((Model.SelectedPropertyRecords?.Any() ?? false)) {
                sb.AppendTable(
                    Model,
                    Model.SelectedPropertyRecords,
                    "HumanMonitoringSelectedPropertiesTable",
                    ViewBag,
                    caption: "Selected population properties and levels.",
                    saveCsv: true,
                    header: true
                );
            } else {
                sb.AppendDescriptionParagraph($"No population properties selected (full population)");
            }

            if (Model.HbmPopulationRecords?.Any() ?? false) {
                sb.AppendTable(
                    Model,
                    Model.HbmPopulationRecords,
                    "HumanMonitoringPopulationCharacteristicsDataTable",
                    ViewBag,
                    caption: "Human monitoring individuals statistics.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            }
        }
    }
}
