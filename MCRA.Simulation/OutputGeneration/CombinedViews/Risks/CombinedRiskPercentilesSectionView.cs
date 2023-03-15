using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedRiskPercentilesSectionView : SectionView<CombinedRiskPercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isMOE = Model.RiskMetric == RiskMetricType.MarginOfExposure;
            if (Model.Percentages.Any() && Model.ExposureModelSummaryRecords.Any()) {
                var percentilesLookup = Model.CombinedExposurePercentileRecords.ToLookup(r => r.IdModel);
                var panelBuilder = new HtmlTabPanelBuilder();

                if ((Model.CombinedExposurePercentileRecords.First().UncertaintyValues?.Any() ?? false)
                        && Model.CombinedExposurePercentileRecords.First().UncertaintyValues.Count > 1) {

                    foreach (var percentage in Model.Percentages) {
                        var violinChartCreator = new CombinedRisksViolinChartCreator(Model, percentage, true, false, false, isMOE);
                        panelBuilder.AddPanel(
                            id: percentage.ToString(),
                            title: $"p{percentage.ToString("F2")}",
                            hoverText: $"p{percentage.ToString("F2")}",
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

                var risk = isMOE ? "margin of exposure" : "hazard index";
                var chartCreator = new CombinedRisksChartCreator(Model, double.NaN, isMOE);
                panelBuilder.AddPanel(
                    id: "Combined overview",
                    title: $"Combined overview {risk}",
                    hoverText: $"Combined overview {risk}",
                    content: ChartHelpers.Chart(
                        name: $"CombinedRisksOverviewViolinChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: chartCreator.Title
                    )
                ); ;
                panelBuilder.RenderPanel(sb);

                sb.Append($"<table class=\"sortable\">");
                sb.Append($"<thead><tr>");
                sb.Append($"<th>Model</th>");
                foreach (var percentage in Model.Percentages) {
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
                    foreach (var percentage in Model.Percentages) {
                        CombinedRiskPercentileRecord value = null;
                        if (percentiles?.TryGetValue(percentage, out value) ?? false) {
                            if (value.HasUncertainty()) {
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
                if (isMOE) {
                    sb.AppendParagraph("No margin of exposure or distributions available.");
                } else {
                    sb.AppendParagraph("No hazard index or distributions available.");
                }
            }
        }
    }
}
