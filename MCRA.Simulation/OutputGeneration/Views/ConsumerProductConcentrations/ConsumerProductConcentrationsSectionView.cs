using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductConcentrationsSectionView : SectionView<ConsumerProductConcentrationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();

            // Chart panels, separate per type of unit
            foreach (var unit in Model.ChartRecords.Keys) {

                var totalSubstances = Model.Records.Select(r => (r.SubstanceCode, r.SubstanceName)).Distinct();
                var description = $"Consumer product concentrations (in {unit.GetShortDisplayName()}) for {totalSubstances.Count()} substances.";

                sb.AppendDescriptionParagraph(description);
                var panelBuilder = new HtmlTabPanelBuilder();

                var percentileRecords = Model.ChartRecords[unit];
                var substances = percentileRecords.Select(r => (r.SubstanceCode, r.SubstanceName)).Distinct();

                foreach (var (substanceCode, substanceName) in substances) {
                    var matrixShortName = $"{substanceCode}-{unit.ToSanitizedString()}";
                    var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                        $"BoxPlotFullPercentiles{matrixShortName}",
                        Model,
                        percentileRecords.Where(c => c.SubstanceCode == substanceCode).ToList(),
                        ViewBag
                    );

                    var sbMatrix = new StringBuilder();
                    sbMatrix.Append("<div class=\"figure-container\">");
                    var chartCreator = new ConsumerProductConcentrationsBoxPlotChartCreator(Model, substanceCode, unit, "substance concentrations in consumer products");
                    sbMatrix.AppendChart(
                        $"ConsumerProductConcentrationsBoxPlotChart{matrixShortName}",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true,
                        chartData: percentileDataSection
                    );
                    sbMatrix.Append("</div>");

                    panelBuilder.AddPanel(
                        id: $"Panel_{substanceName}-{unit.ToSanitizedString()}",
                        title: $"{substanceName}",
                        hoverText: $"{substanceName}",
                        content: new HtmlString(sbMatrix.ToString())
                    );
                }
                panelBuilder.RenderPanel(sb);
            }

            // Table
            var caption = "Consumer product concentrations per consumer product and substance.";
            sb.AppendTable(
                    Model,
                    Model.Records,
                    "ConsumerProductConcentrationsTable",
                    ViewBag,
                    caption: caption,
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
        }
    }
}
