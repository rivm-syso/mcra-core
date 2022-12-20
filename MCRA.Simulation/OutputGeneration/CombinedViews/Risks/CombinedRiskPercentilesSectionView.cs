using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedRiskPercentilesSectionView : SectionView<CombinedRiskPercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Percentages.Any() && Model.ExposureModelSummaryRecords.Any()) {
                var percentilesLookup = Model.CombinedExposurePercentileRecords.ToLookup(r => r.IdModel);
                var chartCreator = new CombinedRisksChartCreator(Model, double.NaN);
                sb.AppendChart(
                    "CombinedRisksPercentilesChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    viewBag: ViewBag,
                    saveChartFile: true,
                    caption: chartCreator.Title
                );
                sb.Append($"<table class=\"sortable\">");
                sb.Append($"<thead><tr>");
                sb.Append($"<th>Model</th>");
                foreach (var percentage in Model.Percentages) {
                    sb.Append($"<th>p{percentage:G3}</th>");
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
                        CombinedExposurePercentileRecord value = null;
                        if (percentiles?.TryGetValue(percentage, out value) ?? false) {
                            if (value.HasUncertainty()) {
                                sb.Append($"<td>{value.UncertaintyMedian:G3}<br />[{value.UncertaintyLowerBound:G3}, {value.UncertaintyUpperBound:G3}]</td>");
                            } else {
                                sb.Append($"<td>{value.Exposure:G3}</td>");
                            }
                        }
                    }
                    sb.Append($"</tr>");
                }
                sb.Append("</tbody></table>");
            } else {
                sb.AppendParagraph("No margin of exposure distributions available.");
            }
        }
    }
}
