using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;


namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionsForIndividualsSectionView : SectionView<ContributionsForIndividualsSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var chartCreator = new IndividualContributionsBySubstanceBoxPlotChartCreator(Model);
            sb.AppendChart(
                "IndividualContributionsBoxPlotChart",
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

