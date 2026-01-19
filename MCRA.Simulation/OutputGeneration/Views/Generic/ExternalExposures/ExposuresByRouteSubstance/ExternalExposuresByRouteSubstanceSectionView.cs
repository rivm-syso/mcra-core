using System.Text;
using MCRA.Simulation.OutputGeneration.Generic.ExternalExposures.ExposuresByRouteSubstance;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposuresByRouteSubstanceSectionView<Tsection> : SectionView<Tsection>
        where Tsection : ExternalExposuresByRouteSubstanceSection {
        public override void RenderSectionHtml(StringBuilder sb) {

            var typeName = typeof(Tsection).Name;
            const string suffix = "Section";
            if (typeName.EndsWith(suffix, StringComparison.Ordinal)) {
                typeName = typeName[..^suffix.Length];
            }

            var positivesRecords = Model.TableRecords
                .Where(r => r.MeanPositives > 0)
                .ToList();
            var missingRouteData = Model.TableRecords
                .GroupBy(r => r.ExposureRoute)
                .Where(records => records.All(r => r.ExposureRoute == null))
                .ToList();
            if (missingRouteData.Count > 0) {
                var description = $"Note: exposure data missing for {missingRouteData.Count} " +
                    $"routes ({string.Join(", ", missingRouteData.Select(r => r.Key))}).";
                sb.AppendWarning(description);
            }

            if (positivesRecords.Count != 0) {
                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var boxPlotRecord in Model.BoxPlotRecords) {
                    var targetCode = boxPlotRecord.Key.GetTypeCode();
                    var targetName = boxPlotRecord.Key.GetDisplayName();

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"{typeName}Percentiles{targetCode}",
                        section: Model,
                        items: boxPlotRecord.Value.ToList(),
                        viewBag: ViewBag
                    );

                    var chartCreator = new ExternalExposuresByRouteSubstanceBoxPlotChartCreator(
                        Model,
                        Model.BoxPlotRecords[boxPlotRecord.Key],
                        boxPlotRecord.Key,
                        Model.ExposureUnit
                    );

                    var numberOfRecords = boxPlotRecord.Value.Count;
                    var warning = Model.BoxPlotRecords[boxPlotRecord.Key].Any(c => c.P95 == 0)
                        ? "The asterisk indicates substances with positive measurements above an upper whisker of zero."
                        : string.Empty;
                    var figCaption = $"{targetName} exposures by substance. " + chartCreator.Title + $" {warning}";
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetCode}",
                        title: $"{targetName} ({numberOfRecords})",
                        hoverText: targetName,
                        content: ChartHelpers.Chart(
                            name: $"{typeName}BoxPlotChart{targetCode}",
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

                if (Model.TableRecords.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                    hiddenProperties.Add("MedianAllMedianPercentile");
                    hiddenProperties.Add("MedianAllLowerBoundPercentile");
                    hiddenProperties.Add("MedianAllUpperBoundPercentile");
                } else {
                    hiddenProperties.Add("MedianAll");
                }

                sb.AppendTable(
                    Model,
                    positivesRecords,
                    $"{typeName}Table",
                    ViewBag,
                    caption: $"Exposures statistics by route and substance.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("No exposures available.");
            }
        }
    }
}
