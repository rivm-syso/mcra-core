using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceSectionView : SectionView<ExposureBySourceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new BoxPlotBySourceChartCreator(
                Model.BoxPlotRecords,
                Model.TargetUnit,
                Model.ShowOutliers
            );

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotBySourceData",
                section: Model,
                items: Model.BoxPlotRecords,
                viewBag: ViewBag
            );
            sb.AppendChart(
                "BoxPlotBySourceChart",
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
                "ExposureBySourceTable",
                ViewBag,
                caption: "Exposure statistics by source (total distribution).",
                saveCsv: true,
                hiddenProperties: null
            );
        }
    }
}
