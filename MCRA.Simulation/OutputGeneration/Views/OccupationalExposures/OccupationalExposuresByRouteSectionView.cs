using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class OccupationalIndividualExposuresPerRouteSectionView : SectionView<OccupationalIndividualExposuresPerRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var positivesRecords = Model.Records
                .Where(r => r.MeanPositives > 0)
                .ToList();
            var missingRouteData = Model.Records
                .GroupBy(r => r.ExposureRoute)
                .Where(records => records.All(r => r.ExposureRoute == null))
                .ToList();
            if (missingRouteData.Count > 0) {
                var description = $"Note: occupational exposure data missing for {missingRouteData.Count} " +
                    $"routes ({string.Join(", ", missingRouteData.Select(r => r.Key))}).";
                sb.AppendWarning(description);
            }

            if (positivesRecords.Any()) {

                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var boxPlotRecord in Model.BoxPlotRecords) {
                    var targetCode = boxPlotRecord.Key.GetTypeCode();
                    var targetName = boxPlotRecord.Key.GetDisplayName();
                    var unit = boxPlotRecord.Value.FirstOrDefault().Unit;

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"OccuptionalExposuresByRoutePercentiles{targetCode}",
                        section: Model,
                        items: boxPlotRecord.Value,
                        viewBag: ViewBag
                    );

                    var chartCreator = new OccupationalExposuresByRouteBoxPlotChartCreator(
                        Model.BoxPlotRecords[boxPlotRecord.Key],
                        boxPlotRecord.Key,
                        Model.SectionId,
                        unit,
                        Model.ShowOutliers
                    );

                    var numberOfRecords = boxPlotRecord.Value.Count;
                    var warning = Model.BoxPlotRecords[boxPlotRecord.Key].Any(c => c.P95 == 0) ? "The asterisk indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
                    var figCaption = $"{targetName} exposures by substance. " + chartCreator.Title + $" {warning}";
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetCode}",
                        title: $"{targetName} ({numberOfRecords})",
                        hoverText: targetName,
                        content: ChartHelpers.Chart(
                            name: $"OccupationalExposuresByRouteBoxPlotChart{targetCode}",
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

                if (Model.Records.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                    hiddenProperties.Add("MedianAllMedianPercentile");
                    hiddenProperties.Add("MedianAllLowerBoundPercentile");
                    hiddenProperties.Add("MedianAllUpperBoundPercentile");
                } else {
                    hiddenProperties.Add("MedianAll");
                }

                sb.AppendTable(
                    Model,
                    positivesRecords,
                    "OccupationalExposuresByRouteTable",
                    ViewBag,
                    caption: $"Exposures by route and substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("No occupational exposures available.");
            }
        }
    }
}
