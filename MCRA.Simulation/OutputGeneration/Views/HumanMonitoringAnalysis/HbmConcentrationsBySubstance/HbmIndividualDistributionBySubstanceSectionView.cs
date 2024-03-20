using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDistributionBySubstanceSectionView : SectionView<HbmIndividualDistributionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var positivesRecords = Model.IndividualRecords
                    .Where(r => r.MeanPositives > 0)
                    .ToList();
            var missingActiveSubstanceData = Model.IndividualRecords
                    .GroupBy(r => r.SubstanceCode)
                    .Where(records => records.All(r => r.SourceSamplingMethods == null))
                    .ToList();
            if (missingActiveSubstanceData.Count > 0) {
                var description = $"Note: concentration data missing for {missingActiveSubstanceData.Count} " +
                    $"active substances ({string.Join(", ", missingActiveSubstanceData.Select(r => r.Key))}).";
                sb.AppendWarning(description);
            }

            if (positivesRecords.Any()) {
                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                    var targetCode = boxPlotRecord.Key.Code;
                    var targetName = boxPlotRecord.Key.GetDisplayName();

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"HbmIndividualDistributionBySubstancePercentiles{targetCode}",
                        section: Model,
                        items: boxPlotRecord.Value,
                        viewBag: ViewBag
                    );

                    var chartCreator = new HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator(
                        Model.HbmBoxPlotRecords[boxPlotRecord.Key],
                        boxPlotRecord.Key,
                        Model.SectionId,
                        ViewBag.GetUnit(targetCode)
                    );

                    var numberOfRecords = boxPlotRecord.Value.Count;
                    var warning = Model.HbmBoxPlotRecords[boxPlotRecord.Key].Any(c => c.P95 == 0) ? "The asterix indicates substances with positive measurements above an upper whisker of zero." : string.Empty;
                    var figCaption = $"{targetName} individual day concentrations by substance. " + chartCreator.Title + $" {warning}";
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetCode}",
                        title: $"{targetName} ({numberOfRecords})",
                        hoverText: targetName,
                        content: ChartHelpers.Chart(
                            name: $"HBMIndividualDistributionBySubstanceBoxPlotChart{targetCode}",
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
                if (Model.IndividualRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                    hiddenProperties.Add("BiologicalMatrix");
                }
                if (Model.IndividualRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                    hiddenProperties.Add("ExposureRoute");
                }
                if (Model.IndividualRecords.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                    hiddenProperties.Add("MedianAllMedianPercentile");
                    hiddenProperties.Add("MedianAllLowerBoundPercentile");
                    hiddenProperties.Add("MedianAllUpperBoundPercentile");
                } else {
                    hiddenProperties.Add("MedianAll");
                }

                sb.AppendTable(
                    Model,
                    positivesRecords,
                    "HbmConcentrationsBySubstanceTable",
                    ViewBag,
                    caption: "Human monitoring individual concentrations by substance.",
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
