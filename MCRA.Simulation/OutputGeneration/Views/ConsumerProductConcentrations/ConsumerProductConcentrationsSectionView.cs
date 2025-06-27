using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductConcentrationsSectionView : SectionView<ConsumerProductConcentrationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();

            var substances = Model.Records.Select(r => (r.SubstanceCode, r.SubstanceName)).Distinct();
            var description = $"Consumer product concentrations for {substances.Count()} substances.";
            
            sb.AppendDescriptionParagraph(description);
            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var (substanceCode, substanceName) in substances) {
                var matrixShortName = substanceCode;
                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                    $"BoxPlotFullPercentiles{matrixShortName}",
                    Model,
                    Model.PercentileRecords.Where(c => c.SubstanceCode == substanceCode).ToList(),
                    ViewBag
                );

                var sbMatrix = new StringBuilder();
                sbMatrix.Append("<div class=\"figure-container\">");
                var chartCreator = new ConsumerProductConcentrationsBoxPlotChartCreator(Model, substanceCode, "substance concentrations in consumer products");
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
                    id: $"Panel_{substanceName}",
                    title: $"{substanceName}",
                    hoverText: substanceName,
                    content: new HtmlString(sbMatrix.ToString())
                );
            }
            panelBuilder.RenderPanel(sb);

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
