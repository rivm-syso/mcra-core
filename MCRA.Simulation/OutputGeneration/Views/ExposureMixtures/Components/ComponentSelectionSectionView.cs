using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ComponentSelectionSectionView : SectionView<ComponentSelectionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            const int columnCount = 3;
            sb.Append("<table><tbody><tr>");
            for (int i = 0; i < Model.ComponentRecords.Count; i++) {
                if (i > 0 && i % columnCount == 0) {
                    sb.Append("</tr><tr>");
                }
                sb.Append("<td>");
                var cutOffItem = Model.SubstancecComponentRecords[i].Where(c => c.NmfValue > 0D).ToList();
                var pieChartCreator = new NMFPieChartCreator(cutOffItem, i);
                sb.AppendChart(
                    $"NMFPieChart{i}",
                    pieChartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    $"Relative contributions of substances to component {i + 1}.",
                    true
                );
                sb.Append("</td>");
            }
            sb.Append("</tr></tbody></table>");

            sb.Append("<div>");
            sb.AppendTable(
               Model,
               Model.ComponentRecords,
               $"NMFHeatMapComponentsTable",
               ViewBag,
               caption: $"Relative contributions of substances to components.",
               saveCsv: true,
               header: true
            );
            sb.Append("</div>");
        }
    }
}
