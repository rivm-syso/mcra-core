using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySubstanceSectionView : SectionView<ExposureBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new ExposureBySubstanceBoxPlotChartCreator(
                Model.ExposureBoxPlotRecords,
                Model.TargetUnit,
                Model.ShowOutliers
            );

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotBySubstanceData",
                section: Model,
                items: Model.ExposureBoxPlotRecords,
                viewBag: ViewBag
            );
            sb.AppendChart(
                "BoxPlotBySubstanceChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true,
                chartData: percentileDataSection
            );


            sb.AppendTable(
                Model,
                Model.ExposureRecords,
                "ExposureBySubstanceTable",
                ViewBag,
                caption: "Exposure statistics by substance (total distribution).",
                saveCsv: true,
                hiddenProperties: null
            );
        }
    }
}
