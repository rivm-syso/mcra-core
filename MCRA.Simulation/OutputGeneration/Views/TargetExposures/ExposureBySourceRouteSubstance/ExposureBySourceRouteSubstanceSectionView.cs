using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureBySourceRouteSubstanceSectionView : SectionView<ExposureBySourceRouteSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new BoxPlotBySourceRouteSubstanceChartCreator(
                Model.BoxPlotRecords,
                Model.TargetUnit,
                Model.ShowOutliers
            );

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotBySourceRouteSubstanceData",
                section: Model,
                items: Model.BoxPlotRecords,
                viewBag: ViewBag
            );
            sb.AppendChart(
                "BoxPlotBySourceRouteSubstanceChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true,
                chartData: percentileDataSection
            );

            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => double.IsNaN(r.RelativePotencyFactor))) {
                hiddenProperties.Add(nameof(ExposureBySourceRouteSubstanceRecord.RelativePotencyFactor));
            }
            if (Model.Records.All(r => r.AssessmentGroupMembership == 1D)) {
                hiddenProperties.Add(nameof(ExposureBySourceRouteSubstanceRecord.AssessmentGroupMembership));
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "ExposureBySourceRouteSubstanceTable",
                ViewBag,
                caption: "Exposure statistics by source, route and substance (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
