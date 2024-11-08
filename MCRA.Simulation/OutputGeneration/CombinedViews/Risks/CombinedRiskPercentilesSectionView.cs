using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedRiskPercentilesSectionView : SectionView<CombinedRiskPercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isHazardExposureRatio = Model.RiskMetric == RiskMetricType.HazardExposureRatio;
            if (Model.Percentages.Any() && Model.ExposureModelSummaryRecords.Any()) {
                var percentilesLookup = Model.CombinedExposurePercentileRecords.ToLookup(r => r.IdModel);
                var panelBuilder = new HtmlTabPanelBuilder();

                if ((Model.CombinedExposurePercentileRecords.First().UncertaintyValues?.Count > 0)
                        && Model.CombinedExposurePercentileRecords.First().UncertaintyValues.Count > 1) {

                    foreach (var percentage in Model.Percentages) {
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

                var chartCreator = new CombinedRisksChartCreator(Model, double.NaN);
                panelBuilder.AddPanel(
                    id: "Combined overview",
                    title: $"Combined risks overview ({Model.RiskMetric.GetDisplayName()})",
                    hoverText: $"Combined risks overview ({Model.RiskMetric.GetDisplayName()})",
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
                sb.AppendParagraph("No risk distributions available.");
            }
        }
    }
}
