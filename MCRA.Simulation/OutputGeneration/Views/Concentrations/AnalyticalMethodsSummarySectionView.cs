using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AnalyticalMethodsSummarySectionView : SectionView<AnalyticalMethodsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var description = $"Number of analytical methods used for the sample analyses of the selected samples: {Model.Records.Count}.";
            sb.AppendDescriptionParagraph(description);

            //Render HTML
            if (Model.Records.Any()) {
                sb.Append(TableHelpers.BuildCustomTableLegend(
                    new List<string>() {
                        "Substance name",
                        "Substance code",
                        "Number of samples analysed",
                        "Substances",
                        "LODs",
                        "LOQs",
                    },
                    new List<string>() {
                        "The substance name of the analytical method",
                        "The substance code of the analytical method",
                        "The number of samples analysed using this analytical method",
                        "The substances belonging to the scope of the analytical method",
                        "The LODs and concentration units",
                        "The LOQs and concentration units",
                    })
                );
                sb.Append($@"<table class='sortable'><thead>
                        <tr>
                            <th>Analytical method name</th>
                            <th>Analytical method code</th>
                            <th>Number of samples analysed</th>
                            <th>Substances</th>
                            <th>LOD</th>
                            <th>LOQ</th>
                        </tr>
                    </thead><tbody>");
                foreach (var item in Model.Records) {
                    sb.Append($"<tr>");
                    sb.Append($"<td rowspan='{item.SubstanceCodes.Count}'>{item.AnalyticalMethodName.ToHtml()}</td>");
                    sb.Append($"<td rowspan='{item.SubstanceCodes.Count}'>{item.AnalyticalMethodCode.ToHtml()}</td>");
                    sb.Append($"<td rowspan='{item.SubstanceCodes.Count}'>{item.NumberOfSamples}</td>");
                    sb.Append($"<td>{item.SubstanceNames[0].ToHtml()} ({item.SubstanceCodes[0].ToHtml()})</td>");
                    if (!double.IsNaN(item.Lods[0])) {
                        sb.Append($"<td>{item.Lods[0]:G3} ({item.ConcentrationUnits[0].ToHtml()})</td>");
                    } else {
                        sb.Append($"<td>-</td>");
                    }
                    if (!double.IsNaN(item.Loqs[0])) {
                        sb.Append($"<td>{item.Loqs[0]:G3} ({item.ConcentrationUnits[0].ToHtml()})</td>");
                    } else {
                        sb.Append($"<td>-</td>");
                    }
                    sb.Append($"</tr>");
                    for (int j = 1; j < item.SubstanceCodes.Count; j++) {
                        sb.Append($"<tr>");
                        sb.Append($"<td>{item.SubstanceNames[j].ToHtml()} ({item.SubstanceCodes[j].ToHtml()})</td>");
                        if (!double.IsNaN(item.Lods[j])) {
                            sb.Append($"<td>{item.Lods[j]:G3} ({item.ConcentrationUnits[j].ToHtml()})</td>");
                        } else {
                            sb.Append($"<td>-</td>");
                        }
                        if (!double.IsNaN(item.Loqs[j])) {
                            sb.Append($"<td>{item.Loqs[j]:G3} ({item.ConcentrationUnits[j].ToHtml()})</td>");
                        } else {
                            sb.Append($"<td>-</td>");
                        }
                        sb.Append($"</tr>");
                    }
                }
                sb.Append("</tbody></table>");
            } else {
                sb.AppendParagraph("No analytical methods available.");
            }
        }
    }
}
