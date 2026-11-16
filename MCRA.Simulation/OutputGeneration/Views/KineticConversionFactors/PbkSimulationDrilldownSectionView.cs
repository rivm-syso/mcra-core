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
                    .GroupBy(r => (r.SimulationId, r.DoseRoute, r.SubstanceCode));
                foreach (var group in recordsGrouped) {
                    var groupKey = $"{group.Key.SubstanceCode}_{group.Key.DoseRoute}_{group.Key.SimulationId}";
                    var groupName = $"{group.First().SubstanceName} - {group.Key.DoseRoute} (#{group.Key.SimulationId})";
                    var records = group.OrderBy(r => r.EndpointName).ToList();
                    var tabSb = new StringBuilder();
                    tabSb.AppendDescriptionParagraph($"PBK simulation output {group.First().SubstanceName}. {group.Key.DoseRoute} dosing of {group.First().Dose:G3} {Model.DoseUnit}.");
                    tabSb.Append("<div class=\"figure-container\">");
                    foreach (var record in records) {
                        var chartCreator = new PbkSimulationTimeSeriesChartCreator(Model, record);
                        tabSb.AppendChart(
                            $"PbkSimulationTimeSeriesChart_{groupKey}_{record.EndpointCode}",
                            chartCreator: chartCreator,
                            //caption: chartCreator.Title,
                            fileType: ChartFileType.Svg,
                            section: Model,
                            viewBag: ViewBag,
                            saveChartFile: false
                        );
                    }
                    tabSb.Append("</div>");
                    targetPanelBuilder.AddPanel(
                        id: $"PbkSimulationDrilldownTab_{groupKey}",
                        title: $"{groupName}",
                        hoverText: $"PBK simulation output #{groupName}",
                        content: new HtmlString(tabSb.ToString())
                    );
                }
            }
            targetPanelBuilder.RenderPanel(sb);
        }
    }
}
