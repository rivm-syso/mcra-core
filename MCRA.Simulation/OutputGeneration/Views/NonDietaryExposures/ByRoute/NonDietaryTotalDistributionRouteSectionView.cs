using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryTotalDistributionRouteSectionView : SectionView<NonDietaryTotalDistributionRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.NonDietaryTotalDistributionRouteRecords.Count > 0 &&  Model.NonDietaryTotalDistributionRouteRecords.First().Contributions.Count > 0;
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
                Model.NonDietaryTotalDistributionRouteRecords,
                "TotalDistributionNonDietaryRouteTable",
                ViewBag,
                caption: "Imputed target doses.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
