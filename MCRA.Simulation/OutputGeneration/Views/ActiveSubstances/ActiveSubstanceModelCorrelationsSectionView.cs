using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ActiveSubstanceModelCorrelationsSectionView : SectionView<ActiveSubstanceModelCorrelationsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            var pearsonChartCreator = new ActiveSubstanceModelPearsonCorrelationsChartCreator(Model);
            sb.AppendChart(
                "ActiveSubstanceModelsPearsonCorrelationsChart",
                pearsonChartCreator,
                ChartFileType.Png,
                Model,
                ViewBag,
                pearsonChartCreator.Title,
                false);

            var spearmanChartCreator = new ActiveSubstanceModelSpearmanCorrelationsChartCreator(Model);
            sb.AppendChart(
                "ActiveSubstanceModelsSpearmanCorrelationsChart",
                spearmanChartCreator,
                ChartFileType.Png,
                Model,
                ViewBag,
                spearmanChartCreator.Title,
                false);
        }
    }
}
