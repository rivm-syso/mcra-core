using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDistributionBySubstanceSectionView : SectionView<HbmIndividualDistributionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.IndividualRecords.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.IndividualDayRecords.All(r => string.IsNullOrEmpty(r.ExposureRoute))) {
                hiddenProperties.Add("ExposureRoute");
            }
            if (Model.IndividualRecords.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))) {
                hiddenProperties.Add("MedianAllMedianPercentile");
                hiddenProperties.Add("MedianAllLowerBoundPercentile");
                hiddenProperties.Add("MedianAllUpperBoundPercentile");
            } else {
                hiddenProperties.Add("MedianAll");
            }

            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    name: $"HbmIndividualDistributionBySubstancePercentiles{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()}",
                    section: Model,
                    items: boxPlotRecord.Value,
                    viewBag: ViewBag
                );

                var unitKey = boxPlotRecord.Key.Code;
                var filenameInsert = $"{boxPlotRecord.Key.BiologicalMatrix}{boxPlotRecord.Key.ExpressionType}";
                var numberOfRecords = boxPlotRecord.Value.Count;

                var chartCreator = new HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator(
                    Model.HbmBoxPlotRecords[boxPlotRecord.Key], 
                    boxPlotRecord.Key, 
                    Model.SectionId, 
                    ViewBag.GetUnit(unitKey)
                );
                var targetName = boxPlotRecord.Key.ExpressionType == ExpressionType.None
                    ? $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()}"
                    : $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()} (standardised by {boxPlotRecord.Key.ExpressionType.ToString().ToLower()})";
                var figCaption = $"{targetName} individual concentrations by substance. " + chartCreator.Title;
                panelBuilder.AddPanel(
                    id: $"Panel_{boxPlotRecord.Key.BiologicalMatrix}_{boxPlotRecord.Key.ExpressionType}",
                    title: $"{targetName} ({numberOfRecords})",
                    hoverText: targetName,
                    content: ChartHelpers.Chart(
                        name: $"HBMIndividualDistributionBySubstance{filenameInsert}BoxPlotChart",
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
                Model.IndividualRecords,
                "HbmConcentrationsBySubstanceTable",
                ViewBag,
                caption: "Human monitoring individual concentrations by substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
