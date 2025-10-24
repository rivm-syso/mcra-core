using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalScenarioExposuresSectionView : SectionView<OccupationalScenarioExposuresSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var positivesRecords = Model.Records
                .Where(r => r.Mean > 0)
                .ToList();

            if (positivesRecords.Any()) {
                var groupedBoxPlotRecords = Model.BoxPlotRecords
                    .GroupBy(r => r.ExposureRoute);

                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var boxPlotRecords in groupedBoxPlotRecords) {
                    var route = boxPlotRecords.Key;

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"OccupationalScenarioExposurePercentiles_{route}",
                        section: Model,
                        items: boxPlotRecords.ToList(),
                        viewBag: ViewBag
                    );

                    var chartCreator = new OccupationalScenarioExposuresBoxPlotChartCreator(
                        records: [.. boxPlotRecords],
                        route: boxPlotRecords.Key,
                        sectionId: Model.SectionId,
                        unit: boxPlotRecords.FirstOrDefault().Unit,
                        showOutliers: true
                    );

                    var numberOfRecords = boxPlotRecords.Count();
                    var warning = boxPlotRecords.Any(c => c.P95 == 0)
                        ? "The asterisk indicates substances with positive measurements above an upper whisker of zero."
                        : string.Empty;

                    var figCaption = $"{route} occupational scenario exposures. {chartCreator.Title}";
                    panelBuilder.AddPanel(
                        id: $"Panel_{route}",
                        title: $"{route} ({numberOfRecords})",
                        hoverText: route,
                        content: ChartHelpers.Chart(
                            name: $"OccupationalScenarioExposuresBoxPlotChart_{route}",
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

                if (Model.Records.All(r => double.IsNaN(r.MedianUncLowerPercentile))) {
                    hiddenProperties.Add("MedianUncMedianPercentile");
                    hiddenProperties.Add("MedianUncLowerPercentile");
                    hiddenProperties.Add("MedianUncUpperPercentile");
                } else {
                    hiddenProperties.Add("Median");
                }
                if (Model.Records.All(r => string.IsNullOrEmpty(r.EstimateType))) {
                    hiddenProperties.Add("EstimateType");
                }

                sb.AppendTable(
                    Model,
                    positivesRecords,
                    "OccupationalScenarioExposuresTable",
                    ViewBag,
                    caption: $"Occupational scenario exposures by route and substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("Occupational scenario exposures available.");
            }
        }
    }
}
