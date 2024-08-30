using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InternalConcentrationTimeSeriesSectionView : SectionView<InternalConcentrationTimeSeriesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            sb.AppendLine("<table>");
            sb.AppendLine("<tr>");
            for (int i = 0; i < Model.Records.Count; i++) {
                if (i > 0 && i % 3 == 0) {
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                }
                sb.AppendLine("<td>");
                var record = Model.Records[i];
                var chartCreator = new InternalConcentrationTimeSeriesChartCreator(Model, record.Compartment);
                sb.AppendChart(
                    $"TimeSeries_{record.Compartment}",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.AppendLine("</td>");
            }
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendTable(
                Model,
                Model.ParameterRecords,
                "ParameterTable",
                ViewBag,
                header: true,
                caption: "Parameter information.",
                saveCsv: true,
                sortable: true
            );
        }
    }
}
