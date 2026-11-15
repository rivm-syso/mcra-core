using System.ComponentModel;
using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using Microsoft.AspNetCore.Html;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class PbkSimulationDrilldownSectionView : SectionView<PbkSimulationDrilldownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var targetPanelBuilder = new HtmlTabPanelBuilder();
            {
                var recordsGrouped = Model.Records
                    .GroupBy(r => r.SimulationId);
                foreach (var group in recordsGrouped) {
                    var records = group.OrderBy(r => r.EndpointName).ToList();
                    var tabSb = new StringBuilder();
                    tabSb.Append("<div class=\"figure-container\">");
                    foreach (var record in records) {
                        var chartCreator = new PbkSimulationTimeSeriesChartCreator(Model, record);
                        tabSb.AppendChart(
                            $"PbkSimulationTimeSeriesChart_{record.SimulationId}_{record.EndpointCode}",
                            chartCreator: chartCreator,
                            fileType: ChartFileType.Svg,
                            section: Model,
                            viewBag: ViewBag,
                            saveChartFile: false
                        );
                    }
                    tabSb.Append("</div>");
                    targetPanelBuilder.AddPanel(
                        id: $"PbkSimulationDrilldownTab_{group.Key}",
                        title: $"Simulation #{group.Key}",
                        hoverText: $"PBK simulation output #{group.Key}",
                        content: new HtmlString(tabSb.ToString())
                    );
                }
            }
            targetPanelBuilder.RenderPanel(sb);
        }
    }
}
