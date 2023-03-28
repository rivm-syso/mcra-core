using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDayDistributionBySubstanceSectionView : SectionView<HbmIndividualDayDistributionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }

            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var boxPlotRecordsKeyValuePair in Model.HbmBoxPlotRecords) {
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "HbmIndividualDayDistributionBySubstancePercentiles", Model, boxPlotRecordsKeyValuePair.Value,
                ViewBag, true, new List<string>());

                var unitKey = Model.CreateUnitKey(boxPlotRecordsKeyValuePair.Key);
                var filenameInsert = $"{boxPlotRecordsKeyValuePair.Key.BiologicalMatrix}{boxPlotRecordsKeyValuePair.Key.ExpressionType}";
                var numberOfRecords = boxPlotRecordsKeyValuePair.Value.Count;

                panelBuilder.AddPanel(
                id: $"Panel {boxPlotRecordsKeyValuePair.Key}",
                title: boxPlotRecordsKeyValuePair.Key.ExpressionType == "None" ? $"Non-standardised ({numberOfRecords})" : $"Standardised by {boxPlotRecordsKeyValuePair.Key.ExpressionType.ToLower()} ({{numberOfRecords}})",
                hoverText: $"Substances concentrations with standardisation {boxPlotRecordsKeyValuePair.Key}",
                content: ChartHelpers.Chart(
                    $"HBMIndividualDayConcentrationBySubstance{filenameInsert}BoxPlotChart",
                    Model,
                    ViewBag,
                    new HbmDayConcentrationsBySubstanceBoxPlotChartCreator(Model, boxPlotRecordsKeyValuePair.Key, ViewBag.GetUnit(unitKey)),
                    ChartFileType.Svg,
                    saveChartFile: true,
                    boxPlotRecordsKeyValuePair.Key.ExpressionType == "None" ? "Concentration values (non-standardised)" : $"Concentration values standardised by {boxPlotRecordsKeyValuePair.Key.ExpressionType.ToLower()}",
                    string.Empty,
                    chartData: percentileDataSection)
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
