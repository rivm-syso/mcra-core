using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;


namespace MCRA.Simulation.OutputGeneration.Views {
    public class IndividualRiskContributionsBySubstanceSectionView : SectionView<IndividualRiskContributionsBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new IndividualContributionsBySubstanceBoxPlotChartCreator(Model);
            sb.AppendChart(
                "IndividualRisksContributionsBySubstanceBoxPlotChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                caption: chartCreator.Title,
                saveChartFile: true
            );
        }
    }
}

