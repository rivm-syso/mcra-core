using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureByRouteSubstanceSectionView : SectionView<ExposureByRouteSubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var groupedBoxPlotRecords = Model.ExposureBoxPlotRecords
                .GroupBy(r => r.ExposureRoute)
                .Select(r => (r.Key, Records: r.ToList()))
                .ToList();

            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var group in groupedBoxPlotRecords) {
                var route = group.Key;

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"BoxPlotByRouteSubstanceData-{route}",
                    section: Model,
                    items: group.Records,
                    viewBag: ViewBag
                );

                var chartCreator = new BoxPlotByRouteSubstanceChartCreator(
                    group.Records,
                    group.Key,
                    Model.SectionId,
                    Model.TargetUnit,
                    Model.ShowOutliers
                );

                var numberOfRecords = group.Records.Count;
                var warning = group.Records.Any(c => c.P95 == 0) ? "The asterisk indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
                var figCaption = $"{route}: exposures by substance. " + chartCreator.Title + $" {warning}";
                panelBuilder.AddPanel(
                    id: $"Panel_{route}",
                    title: $"{route} ({numberOfRecords})",
                    hoverText: route,
                    content: ChartHelpers.Chart(
                        name: $"BoxPlotByRouteSubstanceChart-{route}",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: figCaption,
                        chartData: percentileDataSection
                    )
                );
            }
            panelBuilder.RenderPanel(sb);

            var hiddenProperties = new List<string>();
            if (Model.ExposureRecords.All(r => double.IsNaN(r.RelativePotencyFactor))) {
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("AssessmentGroupMembership");
            }
            sb.AppendTable(
                Model,
                Model.ExposureRecords,
                "ExposureByRouteSubstanceTable",
                ViewBag,
                caption: "Exposure statistics by route and substance (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
