using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConversionSummarySectionView : SectionView<ConversionSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendDescriptionTable(
                "ConversionSummary",
                Model.SectionId,
                Model,
                ViewBag,
                header: false,
                showLegend: true,
                caption: "Conversion statistics"
            );
            sb.AppendTable(
                Model,
                Model.ConversionStepStatistics,
                "ConversionStepStatisticsTable",
                ViewBag,
                caption: "Summary of conversion steps.",
                header: true,
                saveCsv: true
            );
            sb.AppendTable(
                Model,
                Model.ConversionPathStatistics,
                "ConversionPathStatisticsTable",
                ViewBag,
                caption: "Summary of conversion paths.",
                header: true,
                saveCsv: true
            );
        }
    }
}
