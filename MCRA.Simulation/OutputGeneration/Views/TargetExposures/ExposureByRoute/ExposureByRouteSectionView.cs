using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureByRouteSectionView : SectionView<ExposureByRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new BoxPlotByRouteChartCreator(
                Model.BoxPlotRecords,
                Model.TargetUnit.GetShortDisplayName(),
                Model.ShowOutliers
            );

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotByRouteData",
                section: Model,
                items: Model.BoxPlotRecords,
                viewBag: ViewBag
            );
            sb.AppendChart(
                "BoxPlotByRouteChart",
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
                Model.Records,
                "ExposureByRouteTable",
                ViewBag,
                caption: "Exposure statistics by route (total distribution).",
                saveCsv: true,
                hiddenProperties: null
            );
        }
    }
}
