using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.CombinedActionSummaries.Risks;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedRiskPercentilesSectionView : SectionView<CombinedRiskPercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.AllPercentages.Count != 0 && Model.ExposureModelSummaryRecords.Count != 0) {
                var panelBuilder = new HtmlTabPanelBuilder();
                var safetyChartCreator = new CombinedRisksSafetyChartCreator(Model);
                panelBuilder.AddPanel(
                    id: "Combined safetychart overview",
                    title: $"Combined risks overview ({Model.RiskMetric.GetDisplayName()})",
                    hoverText: $"Combined risks overview ({Model.RiskMetric.GetDisplayName()})",
                    content: ChartHelpers.Chart(
                        name: $"CombinedRisksOverviewSafetyChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: safetyChartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: safetyChartCreator.Title
                    )
                );
                
                if (Model.CombinedExposurePercentileRecords.First().UncertaintyValues?.Count > 1) {

                    foreach (var percentage in Model.DisplayPercentages) {
                        var violinChartCreator = new CombinedRisksViolinChartCreator(Model, percentage, true, false, false);
                        panelBuilder.AddPanel(
                            id: percentage.ToString(),
                            title: $"p{percentage:F2}",
                            hoverText: $"p{percentage:F2}",
                            content: ChartHelpers.Chart(
                                name: $"CombinedRisksViolinPercentile_{percentage}Chart",
                                section: Model,
                                viewBag: ViewBag,
                                chartCreator: violinChartCreator,
                                fileType: ChartFileType.Svg,
                                saveChartFile: true,
                                caption: violinChartCreator.Title
                            )
                        );
                    }
                }

                panelBuilder.RenderPanel(sb);

                var percentilesLookup = Model.CombinedExposurePercentileRecords.ToLookup(r => r.IdModel);
                sb.Append($"<table class=\"sortable\">");
                sb.Append($"<caption>Risk characterisation ratio ({Model.RiskMetric.GetShortDisplayName()}) at different percentiles of the risk distribution.</caption>");
                sb.Append($"<thead><tr>");
                sb.Append($"<th>Population</th>");
                foreach (var percentage in Model.DisplayPercentages) {
                    sb.Append($"<th>p{percentage:F2}</th>");
                }
                sb.Append($"</tr></thead>");
                sb.Append($"<tbody>");
                foreach (var item in Model.ExposureModelSummaryRecords) {
                    var percentiles = percentilesLookup.Contains(item.Id)
                        ? percentilesLookup[item.Id].ToDictionary(r => r.Percentage)
                        : null;
                    sb.Append($"<tr>");
                    sb.Append($"<th>{item.Name}</th>");
                    foreach (var percentage in Model.DisplayPercentages) {
                        if (percentiles?.TryGetValue(percentage, out var value) ?? false) {
                            if (value.HasUncertainty) {
                                sb.Append($"<td>{value.UncertaintyMedian:G3}<br />[{value.UncertaintyLowerBound:G3}, {value.UncertaintyUpperBound:G3}]</td>");
                            } else {
                                sb.Append($"<td>{value.Risk:G3}</td>");
                            }
                        }
                    }
                    sb.Append($"</tr>");
                }
                sb.Append("</tbody></table>");
            } else {
                sb.AppendParagraph("No risk distributions available.");
            }
        }
    }
}
