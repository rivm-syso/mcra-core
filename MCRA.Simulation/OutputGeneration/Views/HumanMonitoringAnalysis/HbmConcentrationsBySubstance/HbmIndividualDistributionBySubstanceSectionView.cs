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
            foreach (var boxPlotRecord in Model.HbmBoxPlotRecords) {
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                "HbmIndividualDistributionBySubstancePercentiles", Model, boxPlotRecord.Value,
                ViewBag, true, new List<string>());

                var unitKey = Model.CreateUnitKey(boxPlotRecord.Key);
                var filenameInsert = $"{boxPlotRecord.Key.BiologicalMatrix}{boxPlotRecord.Key.ExpressionType}";
                var numberOfRecords = boxPlotRecord.Value.Count;

                panelBuilder.AddPanel(
                    id: $"Panel_{boxPlotRecord.Key.BiologicalMatrix}_{boxPlotRecord.Key.ExpressionType}",
                    title: boxPlotRecord.Key.ExpressionType == "None"
                        ? $"{boxPlotRecord.Key.BiologicalMatrix} concentrations ({numberOfRecords})"
                        : $"{boxPlotRecord.Key.BiologicalMatrix} concentrations (standardised by {boxPlotRecord.Key.ExpressionType.ToLower()}) ({numberOfRecords})",
                    hoverText: $"Substances concentrations with standardisation {boxPlotRecord.Key}",
                    content: ChartHelpers.Chart(
                        name: $"HBMIndividualDistributionBySubstance{filenameInsert}BoxPlotChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: new HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator(Model, boxPlotRecord.Key, ViewBag.GetUnit(unitKey)),
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: boxPlotRecord.Key.ExpressionType == "None"
                            ? $"{boxPlotRecord.Key.BiologicalMatrix} concentrations"
                            : $"{boxPlotRecord.Key.BiologicalMatrix} concentrations (standardised by {boxPlotRecord.Key.ExpressionType.ToLower()})",
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
                caption: "Human monitoring individual concentrations by substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
