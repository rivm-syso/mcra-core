using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration.CombinedViews {
    public class CombinedDietaryExposurePercentilesSectionView : SectionView<CombinedDietaryExposurePercentilesSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.Percentages.Count != 0 && Model.ModelSummaryRecords.Count != 0) {
                var percentilesLookup = Model.CombinedPercentileRecords.ToLookup(r => r.IdModel);
                var panelBuilder = new HtmlTabPanelBuilder();

                var chartCreator = new CombinedDietaryExposuresChartCreator(Model, double.NaN);
                panelBuilder.AddPanel(
                    id: "CombinedDietaryOverview",
                    title: $"Combined dietary exposure overview ",
                    hoverText: $"Combined dietary exposure overview ",
                    content: ChartHelpers.Chart(
                        name: $"CombinedDietaryExposureOverviewChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: chartCreator.Title
                    )
                );

                if (Model.CombinedPercentileRecords.First()?.UncertaintyValues?.Count > 1) {
                    var count = Model.CombinedPercentileRecords.First().UncertaintyValues.Count;
                    foreach (var percentage in Model.Percentages) {
                        var violinChartCreator = new CombinedDietaryExposureViolinChartCreator(Model, percentage, true, false, false);
                        panelBuilder.AddPanel(
                            id: percentage.ToString(),
                            title: $"p{percentage}",
                            hoverText: $"p{percentage}",
                            content: ChartHelpers.Chart(
                                name: $"CombinedDietaryExposureViolinPercentile_{percentage}Chart",
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

                sb.Append($"<table class=\"sortable\">");
                sb.Append($"<caption>Dietary exposures ({Model.ExposureUnit.GetShortDisplayName()}) at different percentiles of " +
                    $"the exposure distribution.</caption>");
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
