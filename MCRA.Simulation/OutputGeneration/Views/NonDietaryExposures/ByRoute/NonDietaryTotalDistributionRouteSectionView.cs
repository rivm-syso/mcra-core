using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryTotalDistributionRouteSectionView : SectionView<NonDietaryTotalDistributionRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.Records.Count > 0 &&  Model.Records.First().Contributions.Count > 0;
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            //Render HTML
            var chartCreator = new NonDietaryTotalDistributionRoutePieChartCreator(Model, isUncertainty);
            sb.AppendChart(
                "NonDietaryTotalDistributionRoutePieChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );
            sb.AppendParagraph("Absorption factors are not used");
            sb.AppendTable(
                Model,
                Model.Records,
                "TotalDistributionNonDietaryRouteTable",
                ViewBag,
                caption: "Non-dietary contributions to the total distribution for routes",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
