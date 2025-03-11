using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceSectionView : SectionView<ExposureBySourceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new BoxPlotBySourceChartCreator(
                Model.ExposureBoxPlotRecords,
                Model.ExposureUnit,
                Model.ShowOutliers
            );

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotBySourceData",
                section: Model,
                items: Model.ExposureBoxPlotRecords,
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
                Model.ExposureRecords,
                "ExposureBySourceTable",
                ViewBag,
                caption: "Exposure statistics by source (total distribution).",
                saveCsv: true,
                hiddenProperties: null
            );
        }
    }
}
