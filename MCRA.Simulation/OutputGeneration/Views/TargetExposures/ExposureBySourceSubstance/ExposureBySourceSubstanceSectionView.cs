using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceSubstanceSectionView : SectionView<ExposureBySourceSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new BoxPlotBySourceSubstanceChartCreator(
                Model.BoxPlotRecords,
                Model.TargetUnit,
                Model.ShowOutliers
            );

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotBySourceSubstanceData",
                section: Model,
                items: Model.BoxPlotRecords,
                viewBag: ViewBag
            );
            sb.AppendChart(
                "BoxPlotBySourceSubstanceChart",
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
                "ExposureBySourceSubstanceTable",
                ViewBag,
                caption: "Exposure statistics by source and substance (total distribution).",
                saveCsv: true,
                hiddenProperties: null
            );
        }
    }
}
