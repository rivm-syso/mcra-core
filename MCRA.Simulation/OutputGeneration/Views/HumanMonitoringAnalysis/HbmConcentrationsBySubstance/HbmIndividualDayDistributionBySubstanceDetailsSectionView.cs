using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDayDistributionBySubstanceDetailsSectionView : SectionView<HbmIndividualDayDistributionBySubstanceDetailsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
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

            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: "HbmIndividualDayDistributionBySubstancePercentiles",
                    section: Model,
                    items: boxPlotRecord.Value,
                    viewBag: ViewBag
                );

                var unitKey = boxPlotRecord.Key.Code;
                var filenameInsert = $"{boxPlotRecord.Key.BiologicalMatrix}{boxPlotRecord.Key.ExpressionType}";
                var numberOfRecords = boxPlotRecord.Value.Count;
                var chartCreator = new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(
                    Model.HbmBoxPlotRecords[boxPlotRecord.Key],
                    boxPlotRecord.Key,
                    Model.SectionId,
                    Model.HbmBoxPlotRecords[boxPlotRecord.Key].FirstOrDefault()?.Unit ?? string.Empty
                );
                var targetName = boxPlotRecord.Key.ExpressionType == ExpressionType.None
                    ? $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()}"
                    : $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()} (standardised by {boxPlotRecord.Key.ExpressionType.ToString().ToLower()})";
                var figCaption = $"{targetName} individual day concentrations by substance. " + chartCreator.Title;
                panelBuilder.AddPanel(
                    id: $"Panel_{boxPlotRecord.Key.BiologicalMatrix}_{boxPlotRecord.Key.ExpressionType}",
                    title: $"{targetName} ({numberOfRecords})",
                    hoverText: targetName,
                    content: ChartHelpers.Chart(
                        name: $"HBMIndividualDayConcentrationBySubstance{filenameInsert}DetailsBoxPlotChart",
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

            //Render HTML
            sb.AppendTable(
                Model,
                Model.IndividualDayRecords,
                "HbmConcentrationsBySubstanceDetailsTable",
                ViewBag,
                caption: $"Human monitoring day concentrations by substance before matrix conversion.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
