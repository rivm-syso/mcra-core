using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InternalVersusExternalExposuresSectionView : SectionView<InternalVersusExternalExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var counter = 0;

            var plot = Model.AbsorptionFactorsPercentiles
                .Select(c => c.ReferenceValues.First())
                .Any(v => !double.IsNaN(v) && v > 0);

            if (plot) {
                sb.Append("<table>");
                sb.Append("<caption>Internal/external ratio calculated for the default individual of the kinetic model.</caption>");
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

                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var item in Model.TargetUnits) {
                    var chartCreator = new InternalVersusExternalExposuresScatterChartCreator(
                        Model,
                        item
                    );
                    panelBuilder.AddPanel(
                        id: item.Target.Code,
                        title: item.GetShortDisplayName(),
                        hoverText: item.GetShortDisplayName(),
                        content: ChartHelpers.Chart(
                            name: $"KineticModelChart{item.Target.Code}",
                            section: Model,
                            viewBag: ViewBag,
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true,
                            caption: chartCreator.Title
                        )
                    );
                }
                panelBuilder.RenderPanel(sb);
            }
        }
    }
}
