using System.Text;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AttributableBodSummarySectionView : SectionView<AttributableBodSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var panelBuilder = new HtmlTabPanelBuilder();

            var panelGroup = Model.Records
                .GroupBy(r => (r.BodIndicator, r.ExposureResponseFunctionCode));

            foreach (var group in panelGroup) {
                var panelSb = new StringBuilder();
                var key = $"{group.Key.BodIndicator}-{group.Key.ExposureResponseFunctionCode}";
                panelSb.AppendTable(
                    Model,
                    [.. group],
                    $"AttributableBodTable_{key}",
                    ViewBag,
                    header: true,
                    caption: $"Attributable burden of disease {key}.",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: ["BodIndicator", "ExposureResponseFunctionCode"]
                );

                var chartCreator = new AttributableBodChartCreator(
                    [.. group],
                    Model.SectionId
                );

                var contentPanel = new HtmlString(
                    ChartHelpers.Chart(
                        name: $"AttributableBodChart{key}",
                        section: Model,
                        viewBag: ViewBag,
                        caption: chartCreator.Title,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true
                    ) + panelSb.ToString()
                );

                panelBuilder.AddPanel(
                    id: $"Panel_{key}",
                    title: $"{key}",
                    hoverText: key,
                        content: contentPanel
                );
            }
            panelBuilder.RenderPanel(sb);
        }
    }
}
