using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDayDistributionBySubstanceDetailsSectionView : SectionView<HbmIndividualDayDistributionBySubstanceDetailsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var positivesRecords = Model.IndividualDayRecords
                .Where(r => r.MeanPositives > 0)
                .ToList();
            if (Model.RestrictedUpperPercentile.HasValue) {
                var upper = Model.RestrictedUpperPercentile.Value;
                sb.AppendWarning("This section cannot be rendered because the sample size is insufficient for reporting the selected percentiles in accordance with the privacy guidelines." +
                    $" For the given sample size, only percentile values below p{upper:#0.##} can be reported.");
            } else {
                if (positivesRecords.Any()) {
                    var panelBuilder = new HtmlTabPanelBuilder();
                    foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                        var targetCode = boxPlotRecord.Key.Code;
                        var targetName = boxPlotRecord.Key.GetDisplayName();

                        var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                            name: $"HbmConcentrationsBySubstanceDetailsPercentiles_{targetCode}",
                            section: Model,
                            items: boxPlotRecord.Value,
                            viewBag: ViewBag
                        );

                        var chartCreator = new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(
                            Model.HbmBoxPlotRecords[boxPlotRecord.Key],
                            boxPlotRecord.Key,
                            Model.SectionId,
                            Model.HbmBoxPlotRecords[boxPlotRecord.Key].FirstOrDefault()?.Unit ?? string.Empty,
                            Model.ShowOutliers
                        );

                        var numberOfRecords = boxPlotRecord.Value.Count;
                        var warning = Model.HbmBoxPlotRecords[boxPlotRecord.Key].Any(c => c.P95 == 0) ? "The asterisk indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
                        var figCaption = $"{targetName} individual day concentrations by substance. " + chartCreator.Title + $" {warning}";
                        panelBuilder.AddPanel(
                            id: $"Panel_{targetCode}",
                            title: $"{targetName} ({numberOfRecords})",
                            hoverText: targetName,
                            content: ChartHelpers.Chart(
                                name: $"HbmConcentrationsBySubstanceDetailsBoxPlots_{targetCode}",
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
                    if (Model.IndividualDayRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                        hiddenProperties.Add("BiologicalMatrix");
                    }
                    if (Model.IndividualDayRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                        hiddenProperties.Add("ExposureRoute");
                    }
                    if (Model.IndividualDayRecords.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                        hiddenProperties.Add("MedianAllMedianPercentile");
                        hiddenProperties.Add("MedianAllLowerBoundPercentile");
                        hiddenProperties.Add("MedianAllUpperBoundPercentile");
                    } else {
                        hiddenProperties.Add("MedianAll");
                    }

                    sb.AppendTable(
                        Model,
                        positivesRecords,
                        "HbmConcentrationsBySubstanceDetailsTable",
                        ViewBag,
                        caption: $"Human biomonitoring individual day concentrations by substance before matrix conversion.",
                        saveCsv: true,
                        header: true,
                        hiddenProperties: hiddenProperties
                    );
                } else {
                    sb.AppendNotification("No concentrations available.");
                }
            }
        }
    }
}