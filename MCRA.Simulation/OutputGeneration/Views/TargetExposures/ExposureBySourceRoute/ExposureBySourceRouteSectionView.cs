using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceRouteSectionView : SectionView<ExposureBySourceRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.ExposureRecords.Count > 0) {
                var chartCreator = new BoxPlotBySourceRouteChartCreator(
                    Model.ExposureBoxPlotRecords,
                    Model.ExposureUnit,
                    Model.ShowOutliers
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"InternalBoxPlotBySourceRouteData",
                    section: Model,
                    items: Model.ExposureBoxPlotRecords,
                    viewBag: ViewBag
                );
                sb.AppendChart(
                    "BoxPlotBySourceRouteChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true,
                    chartData: percentileDataSection
                );
                sb.AppendTable(
                   Model,
                   Model.ExposureRecords,
                   "ExposureBySourceRouteTable",
                   ViewBag,
                   caption: $"Exposures statistics by source and route (total distribution).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendParagraph("No internal exposures by source and route available.");
            }
        }
    }
}
