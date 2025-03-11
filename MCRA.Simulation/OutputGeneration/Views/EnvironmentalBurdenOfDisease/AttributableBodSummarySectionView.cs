using System.Text;
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
                    group.ToList(),
                    $"AttributableBodTable_{key}",
                    ViewBag,
                    header: true,
                    caption: $"Attributable burden of disease {key}.",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: ["BodIndicator", "ExposureResponseFunctionCode"]
                );

                panelBuilder.AddPanel(
                    id: $"Panel_{key}",
                    title: $"{key}",
                    hoverText: key,
                    content: new HtmlString(panelSb.ToString())
                );
            }
            panelBuilder.RenderPanel(sb);
        }
    }
}
