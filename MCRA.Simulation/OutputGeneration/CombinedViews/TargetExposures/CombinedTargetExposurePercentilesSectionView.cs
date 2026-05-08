using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Utils.ExtensionMethods;

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

                var percentileDataSection = DataSectionHelper.CreateCsvDataSection(
                     name: $"CombinedTargetExposurePercentilesData",
                     section: Model,
                     items: Model.CombinedPercentileRecords,
                     viewBag: ViewBag
                );

                sb.Append($"<table ");
                sb.Append($" class=\"sortable\"");
                sb.Append($" csv-download-id=\"{percentileDataSection.SectionGuid:N}\"");
                sb.Append($" csv-download-name=\"{percentileDataSection.TableName}\">");
                sb.Append($"<caption>Exposures in {Model.ExposureUnit.GetDisplayName()}</caption>");
                sb.Append($"<thead><tr>");
                sb.Append($"<th>Population</th>");
                foreach (var percentage in Model.Percentages) {
                    sb.Append($"<th>p{percentage:G3}</th>");
                }
                sb.Append($"</tr></thead>");
                sb.Append($"<tbody>");
                var emDash = "\u2014";
                foreach (var item in Model.ModelSummaryRecords) {
                    var percentiles = percentilesLookup.Contains(item.Id)
                        ? percentilesLookup[item.Id].ToDictionary(r => r.Percentage)
                        : null;
                    sb.Append($"<tr>");
                    sb.Append($"<td>{item.Name}</td>");
                    foreach (var percentage in Model.Percentages) {
                        if (percentiles?.TryGetValue(percentage, out var value) ?? false) {
                            if (value.HasUncertainty) {
                                sb.Append($"<td>&nbsp;{value.UncertaintyMedian.FormatAdaptive()}&nbsp;&nbsp;[{value.UncertaintyLowerBound.FormatAdaptive()}{emDash}{value.UncertaintyUpperBound.FormatAdaptive()}]&nbsp;</td>");
                            } else {
                                sb.Append($"<td>&nbsp;{value.Value.FormatAdaptive()}&nbsp;</td>");
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
