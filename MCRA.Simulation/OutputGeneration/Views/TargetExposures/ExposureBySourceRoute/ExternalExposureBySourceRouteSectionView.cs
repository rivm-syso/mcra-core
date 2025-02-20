using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposuresBySourceRouteSectionView : SectionView<ExternalExposuresBySourceRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.ExposureRecords.Count > 0) {
                var chartCreator = new ExternalBoxPlotBySourceRouteChartCreator(
                    Model.ExposureBoxPlotRecords,
                    Model.TargetUnit.GetShortDisplayName(),
                    Model.ShowOutliers
                );

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"BoxPlotBySourceRouteData",
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
                   "ExternalExposureBySourceRouteTable",
                   ViewBag,
                   caption: $"External exposures statistics by source and route (total distribution).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendParagraph("No external exposures by source and route available.");
            }
        }
    }
}
