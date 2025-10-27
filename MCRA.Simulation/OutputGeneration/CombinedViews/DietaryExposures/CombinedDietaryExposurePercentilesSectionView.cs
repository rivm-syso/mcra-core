using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedDietaryExposurePercentilesSectionView : SectionView<CombinedDietaryExposurePercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Percentages.Any() && Model.ExposureModelSummaryRecords.Any()) {
                var percentilesLookup = Model.CombinedExposurePercentileRecords.ToLookup(r => r.IdModel);
                var chartCreator = new CombinedDietaryExposuresChartCreator(Model, double.NaN);
                sb.AppendChart(
                    "CombinedDietaryExposurePercentilesChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    viewBag: ViewBag,
                    saveChartFile: true,
                    caption: chartCreator.Title
                );

                sb.Append($"<table class=\"sortable\">");
                sb.Append($"<caption>Exposures in {Model.ExposureUnit.GetShortDisplayName()}</caption>");
                sb.Append($"<thead><tr>");
                sb.Append($"<th>Population</th>");
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
                sb.AppendParagraph("No exposure distributions available.");
            }
        }
    }
}
