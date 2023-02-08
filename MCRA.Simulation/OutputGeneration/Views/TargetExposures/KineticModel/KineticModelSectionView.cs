using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelSectionView : SectionView<KineticModelSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var counter = 0;

            //Render HTML
            sb.Append("<table>");
            sb.AppendTableRow("PBPK modeling", $"{Model.CodeModel}, {Model.Description}");
            sb.AppendTableRow("Substance", Model.CodeSubstance);
            sb.AppendTableRow("Output", Model.OutputDescription);
            if (Model.ExposureRoutes != null) {
                sb.AppendTableRow("Modelled exposure route(s)", string.Join(", ", Model.ExposureRoutes));
            }
            sb.Append("</table>");

            var plot = Model.AbsorptionFactorsPercentiles
                .Select(c => c.ReferenceValues.First())
                .Any(v => !double.IsNaN(v) && v > 0);

            if (plot) {
                sb.Append("<table>");
                sb.Append("<caption>Internal/external ratio calculated for the default individual of the kinetic model (commonly with a bodyweight of 70kg)</caption>");
                sb.Append("<thead>");
                sb.AppendHeaderRow("Exposure route", "Nominal", $"p{Model.UncertaintyLowerLimit}", $"p{Model.UncertaintyUpperLimit}");
                sb.Append("</thead><tbody>");
                foreach (var item in Model.AbsorptionFactorsPercentiles) {
                    var point = item.First();
                    var factor = double.IsNaN(point.ReferenceValue) ? "-" : $"{point.ReferenceValue:G3}";
                    var lowerBound = double.IsNaN(point.MedianUncertainty) ? "-" : $"{point.Percentile(Model.UncertaintyLowerLimit):G3}";
                    var upperBound = double.IsNaN(point.MedianUncertainty) ? "-" : $"{point.Percentile(Model.UncertaintyUpperLimit):G3}";
                    sb.AppendTableRow(Model.AllExposureRoutes[counter], $"{point.ReferenceValue:G3}", lowerBound, upperBound);
                    counter++;
                }
                sb.Append("</tbody></table>");
                var chartCreator = new KineticModelChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), ViewBag.GetUnit("ExternalExposureUnit"));
                sb.AppendChart(
                    "KineticModelChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
        }
    }
}
