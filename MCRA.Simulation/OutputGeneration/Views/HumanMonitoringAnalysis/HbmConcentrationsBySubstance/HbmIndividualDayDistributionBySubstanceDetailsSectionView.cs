using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDayDistributionBySubstanceDetailsSectionView : SectionView<HbmIndividualDayDistributionBySubstanceDetailsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.IndividualDayRecords.Any()) {
                var panelBuilder = new HtmlTabPanelBuilder();
                foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                    var targetCode = boxPlotRecord.Key.Code;
                    var targetName = boxPlotRecord.Key.GetDisplayName();

                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        name: $"HbmIndividualDayDistributionBySubstancePercentiles{targetCode}",
                        section: Model,
                        items: boxPlotRecord.Value,
                        viewBag: ViewBag
                    );

                    var chartCreator = new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(
                        Model.HbmBoxPlotRecords[boxPlotRecord.Key],
                        boxPlotRecord.Key,
                        Model.SectionId,
                        Model.HbmBoxPlotRecords[boxPlotRecord.Key].FirstOrDefault()?.Unit ?? string.Empty
                    );

                    var numberOfRecords = boxPlotRecord.Value.Count;
                    var figCaption = $"{targetName} individual day concentrations by substance. " + chartCreator.Title;
                    panelBuilder.AddPanel(
                        id: $"Panel_{targetCode}",
                        title: $"{targetName} ({numberOfRecords})",
                        hoverText: targetName,
                        content: ChartHelpers.Chart(
                            name: $"HBMIndividualDayConcentrationBySubstanceDetailsBoxPlotChart{targetCode}",
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
                    Model.IndividualDayRecords,
                    "HbmConcentrationsBySubstanceDetailsTable",
                    ViewBag,
                    caption: $"Human monitoring individual day concentrations by substance before matrix conversion.",
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
