using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SoilExposuresByRouteSectionView : SectionView<SoilExposuresByRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var positivesRecords = Model.SoilExposuresByRouteRecords
                .Where(r => r.MeanPositives > 0)
                .ToList();
            var missingRouteData = Model.SoilExposuresByRouteRecords
                .GroupBy(r => r.ExposureRoute)
                .Where(records => records.All(r => r.ExposureRoute == null))
                .ToList();
            if (missingRouteData.Count > 0) {
                var description = $"Note: soil exposure data missing for {missingRouteData.Count} " +
                    $"routes ({string.Join(", ", missingRouteData.Select(r => r.Key))}).";
                sb.AppendWarning(description);
            }

            if (positivesRecords.Any()) {

                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var boxPlotRecord in Model.SoilExposuresBoxPlotRecords) {
                    var targetCode = boxPlotRecord.Key.GetTypeCode();
                    var targetName = boxPlotRecord.Key.GetDisplayName();
                    var unit = boxPlotRecord.Value.FirstOrDefault().Unit;

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"SoilExposuresByRoutePercentiles{targetCode}",
                        section: Model,
                        items: boxPlotRecord.Value,
                        viewBag: ViewBag
                    );

                    var chartCreator = new SoilExposuresByRouteBoxPlotChartCreator(
                        Model.SoilExposuresBoxPlotRecords[boxPlotRecord.Key],
                        boxPlotRecord.Key,
                        Model.SectionId,
                        unit,
                        Model.ShowOutliers
                    );

                    var numberOfRecords = boxPlotRecord.Value.Count;
                    var warning = Model.SoilExposuresBoxPlotRecords[boxPlotRecord.Key].Any(c => c.P95 == 0) ? "The asterisk indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
                    var figCaption = $"{targetName} exposures by substance. " + chartCreator.Title + $" {warning}";
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetCode}",
                        title: $"{targetName} ({numberOfRecords})",
                        hoverText: targetName,
                        content: ChartHelpers.Chart(
                            name: $"SoilExposuresByRouteBoxPlotChart{targetCode}",
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

                if (Model.SoilExposuresByRouteRecords.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                    hiddenProperties.Add("MedianAllMedianPercentile");
                    hiddenProperties.Add("MedianAllLowerBoundPercentile");
                    hiddenProperties.Add("MedianAllUpperBoundPercentile");
                } else {
                    hiddenProperties.Add("MedianAll");
                }

                sb.AppendTable(
                    Model,
                    positivesRecords,
                    "SoilExposuresByRouteTable",
                    ViewBag,
                    caption: $"Exposures by route by substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("No soil exposures available.");
            }
        }
    }
}
