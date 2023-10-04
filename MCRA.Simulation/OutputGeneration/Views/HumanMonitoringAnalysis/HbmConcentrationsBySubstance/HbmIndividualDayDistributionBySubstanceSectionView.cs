using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDayDistributionBySubstanceSectionView : SectionView<HbmIndividualDayDistributionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }
            if (Model.Records.All(r => double.IsNaN(r.MedianAllLowerBoundPercentile))){
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
                    viewBag: ViewBag, 
                    true,
                    new List<string>()
                );

                var unitKey = boxPlotRecord.Key.Code;
                var filenameInsert = $"{boxPlotRecord.Key.BiologicalMatrix}{boxPlotRecord.Key.ExpressionType}";
                var numberOfRecords = boxPlotRecord.Value.Count;

                panelBuilder.AddPanel(
                    id: $"Panel_{boxPlotRecord.Key.BiologicalMatrix}_{boxPlotRecord.Key.ExpressionType}",
                    title: boxPlotRecord.Key.ExpressionType == ExpressionType.None
                        ? $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()} concentrations ({numberOfRecords})" 
                        : $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()} concentrations (standardised by {boxPlotRecord.Key.ExpressionType.ToString().ToLower()}) ({numberOfRecords})",
                    hoverText: $"Substances concentrations with standardisation {boxPlotRecord.Key}",
                    content: ChartHelpers.Chart(
                        name: $"HBMIndividualDayConcentrationBySubstance{filenameInsert}BoxPlotChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(Model, boxPlotRecord.Key, ViewBag.GetUnit(unitKey)),
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: boxPlotRecord.Key.ExpressionType == ExpressionType.None
                            ? $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()} concentrations" 
                            : $"{boxPlotRecord.Key.BiologicalMatrix.GetDisplayName()} concentrations (standardised by {boxPlotRecord.Key.ExpressionType.ToString().ToLower()})",
                        chartData: percentileDataSection
                    )
                );
            }
            panelBuilder.RenderPanel(sb);

            //Render HTML
            sb.AppendTable(
                Model,
                Model.Records,
                "HbmConcentrationsBySubstanceTable",
                ViewBag,
                caption: $"Human monitoring day concentrations by substance. Concentrations are expressed per {ViewBag.GetUnit("MonitoringConcentrationUnit")}",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
