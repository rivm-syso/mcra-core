using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmIndividualDistributionBySubstanceSectionView : SectionView<HbmIndividualDistributionBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.BiologicalMatrix))) {
                hiddenProperties.Add("BiologicalMatrix");
            }

            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var boxPlotRecordsKeyValuePair in Model.HbmBoxPlotRecords) {
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "HbmIndividualDistributionBySubstancePercentiles", Model, boxPlotRecordsKeyValuePair.Value,
                ViewBag, true, new List<string>());

                var unitKey = Model.CreateUnitKey(boxPlotRecordsKeyValuePair.Key);
                var filenameInsert = $"{boxPlotRecordsKeyValuePair.Key.BiologicalMatrix}{boxPlotRecordsKeyValuePair.Key.ExpressionType}";
                var numberOfRecords = boxPlotRecordsKeyValuePair.Value.Count;

                panelBuilder.AddPanel(
                id: $"Panel {boxPlotRecordsKeyValuePair.Key}",
                title: boxPlotRecordsKeyValuePair.Key.ExpressionType == "None" ? $"Non-standardised ({numberOfRecords})" : $"Standardised by {boxPlotRecordsKeyValuePair.Key.ExpressionType.ToLower()} ({numberOfRecords})",
                hoverText: $"Substances concentrations with standardisation {boxPlotRecordsKeyValuePair.Key}",
                content: ChartHelpers.Chart(
                    $"HBMIndividualDistributionBySubstance{filenameInsert}BoxPlotChart",
                    Model,
                    ViewBag,
                    new HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator(Model, boxPlotRecordsKeyValuePair.Key, ViewBag.GetUnit(unitKey)),
                    ChartFileType.Svg,
                    saveChartFile: true,
                    boxPlotRecordsKeyValuePair.Key.ExpressionType == "None" ? "Concentration values non-standardised" : $"Concentration values standardised by {boxPlotRecordsKeyValuePair.Key.ExpressionType.ToLower()}",
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
                caption: "Human monitoring individual concentrations by substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
