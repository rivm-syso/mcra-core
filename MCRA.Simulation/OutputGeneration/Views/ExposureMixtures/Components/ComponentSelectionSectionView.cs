using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ComponentSelectionSectionView : SectionView<ComponentSelectionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            sb.AppendDescriptionParagraph($"Component {Model.ComponentNumber}: sparseness constraint = {Model.Sparseness.ToString("F3")}, number of iterations = {Model.NumberOfIterations}.");

            var cutOffItem = Model.SubstancecComponentRecords.Where(c => c.NmfValue > 0.00).ToList();
            sb.Append("<div class=\"figure-container\">");
            sb.Append("<figure>");
            var pieChartCreator = new NMFPieChartCreator(cutOffItem, Model.ComponentNumber);
            sb.AppendChart(
                $"NMFPieChart{Model.ComponentNumber}",
                pieChartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                $"Relative contributions of substances to component {Model.ComponentNumber}.",
                true
            );
            sb.Append("</figure>");

            sb.Append("<figure>");
            sb.AppendTable(
               Model,
               cutOffItem,
               $"NMFHeatMapTable{Model.ComponentNumber}",
               ViewBag,
               caption: $"Relative contributions of substances to component {Model.ComponentNumber}.",
               saveCsv: true,
               header: true
            );
            sb.Append("</figure>");

            sb.Append("</div>");
        }
    }
}
