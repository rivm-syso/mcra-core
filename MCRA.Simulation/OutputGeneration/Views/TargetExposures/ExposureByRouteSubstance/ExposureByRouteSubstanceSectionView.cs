using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureByRouteSubstanceSectionView : SectionView<ExposureByRouteSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                name: $"BoxPlotByRouteSubstanceData",
                section: Model,
                items: Model.ExposureBoxPlotRecords,
                viewBag: ViewBag
            );

            var chartCreator = new ExposuresByRouteSubstanceBoxPlotCreator(
                Model.ExposureBoxPlotRecords,
                Model.SectionId,
                Model.TargetUnit,
                Model.ShowOutliers
            );
            var numberOfRecords = Model.ExposureBoxPlotRecords.Count;
            var warning = Model.ExposureBoxPlotRecords.Any(c => c.P95 == 0) ? "The asterisk indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
            var figCaption = $"Exposures by substance. " + chartCreator.Title + $" {warning}";
            sb.AppendChart(
                name: $"ExposureByRouteSubstanceBoxPlot",
                section: Model,
                viewBag: ViewBag,
                chartCreator: chartCreator,
                fileType: ChartFileType.Svg,
                saveChartFile: true,
                caption: figCaption,
                chartData: percentileDataSection
            );

            var hiddenProperties = new List<string>();
            if (Model.ExposureRecords.All(r => double.IsNaN(r.RelativePotencyFactor))) {
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("AssessmentGroupMembership");
            }
            var records = Model.ExposureRecords.Where(r => r.MeanAll > 0).ToList();
            sb.AppendTable(
                Model,
                records,
                "ExposureByRouteSubstanceTable",
                ViewBag,
                caption: "Exposure statistics by route and substance (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
