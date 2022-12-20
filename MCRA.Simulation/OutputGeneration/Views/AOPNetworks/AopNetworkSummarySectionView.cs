using MCRA.Simulation.OutputGeneration.ActionSummaries.AOPNetworks;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class AopNetworkSummarySectionView : SectionView<AopNetworkSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            sb.AppendDescriptionTable(
                "AOPNetworksSummarySectionRecordsTable",
                Model.SectionId,
                Model,
                ViewBag,
                header: false
            );

            var caption = $"AOP network {Model.AOPName} ({Model.AOPCode}).";
            var cyclicKers = Model.KeyEventRelationships.Where(r => r.IsCyclic).ToList();
            var indirectKers = Model.KeyEventRelationships.Where(r => r.IsIndirect).ToList();
            if (cyclicKers.Any() && indirectKers.Any()) {
                caption += " Note: the AOP network contains cyclic and indirect key event relationships that are not shown in the graph.";
            } else if (cyclicKers.Any()) {
                caption += " Note: the AOP network contains cyclic key event relationships that are not shown in the graph.";
            } else if (indirectKers.Any()) {
                caption += " Note: the AOP network contains indirect key event relationships that are not shown in the graph.";
            }

            var chartCreator = new AopNetworkGraphCreator(Model, 400);
            sb.AppendChart(
                "AopNetworkGraph",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption,
                true
            );
        }
    }
}
