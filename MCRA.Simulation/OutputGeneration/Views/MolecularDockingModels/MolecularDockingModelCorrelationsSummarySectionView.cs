using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MolecularDockingModelCorrelationsSummarySectionView : SectionView<MolecularDockingModelCorrelationsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            var pearsonChartCreator = new MolecularDockingModelPearsonCorrelationsChartCreator(Model);
            sb.AppendChart(
                "MolecularDockingModelPearsonCorrelationsChart",
                pearsonChartCreator,
                ChartFileType.Png,
                Model,
                ViewBag,
                pearsonChartCreator.Title,
                false);

            var spearmanChartCreator = new MolecularDockingModelSpearmanCorrelationsChartCreator(Model);
            sb.AppendChart(
                "MolecularDockingModelSpearmanCorrelationsChart",
                spearmanChartCreator,
                ChartFileType.Png,
                Model,
                ViewBag,
                spearmanChartCreator.Title,
                false);
        }
    }
}
