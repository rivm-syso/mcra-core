using MCRA.Utils.ExtensionMethods;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedTargetExposurePercentilesSectionView : SectionView<CombinedTargetExposurePercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Percentages.Count != 0 && Model.ModelSummaryRecords.Count != 0) {
                var percentilesLookup = Model.CombinedPercentileRecords.ToLookup(r => r.IdModel);
                var chartCreator = new CombinedTargetExposuresChartCreator(Model, double.NaN);
                sb.AppendChart(
                    "CombinedExposurePercentilesChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    viewBag: ViewBag,
                    saveChartFile: true,
                    caption: chartCreator.Title
                );
                sb.Append($"<table class=\"sortable\">");
                sb.Append($"<caption>Exposures in {Model.ExposureUnit.GetDisplayName()}</caption>");
                sb.Append($"<thead><tr>");
                sb.Append($"<th>Population</th>");
                foreach (var percentage in Model.Percentages) {
                    sb.Append($"<th>p{percentage:G4}</th>");
                }
                sb.Append($"</tr></thead>");
                sb.Append($"<tbody>");
                foreach (var item in Model.ModelSummaryRecords) {
                    var percentiles = percentilesLookup.Contains(item.Id)
                        ? percentilesLookup[item.Id].ToDictionary(r => r.Percentage)
                        : null;
                    sb.Append($"<tr>");
                    sb.Append($"<th>{item.Name}</th>");
                    foreach (var percentage in Model.Percentages) {
                        if (percentiles?.TryGetValue(percentage, out var value) ?? false) {
                            if (value.HasUncertainty) {
                                sb.Append($"<td>{value.UncertaintyMedian:G4}<br />[{value.UncertaintyLowerBound:G4}, {value.UncertaintyUpperBound:G4}]</td>");
                            } else {
                                sb.Append($"<td>{value.Value:G4}</td>");
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
