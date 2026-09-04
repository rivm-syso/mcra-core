using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConsumerProductExposureDistributionSectionView : SectionView<ConsumerProductExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var panelBuilder = new HtmlTabPanelBuilder();
            foreach (var record in Model.FittedPercentileRecords) {
                var panelSb = new StringBuilder();
                var chartCreator = new ConsumerProductPercentilesFitChartCreator(Model.SectionId, record, record.Unit);
                panelSb.AppendChart(
                    $"{record.ExposureSetCode}DistributionChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    $"{record.ExposureSetCode}: {record.SubstanceName}, {record.Route}",
                    true
                );
                //Render HTML
                panelSb.AppendTable(
                    Model,
                    [record],
                    $"{record.ExposureSetCode}ConsumerProductExposureFittedTable",
                    ViewBag,
                    caption: "Fitted exposure distributions.",
                    saveCsv: true,
                    header: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );

                panelBuilder.AddPanel(
                    id: $"Panel_{record.ExposureSetCode}",
                    title: record.ExposureSetCode,
                    hoverText: record.ExposureSetCode,
                    content: new HtmlString(panelSb.ToString())
                );
            }
            panelBuilder.RenderPanel(sb, collapseSingleTab: true);
        }
    }
}
