using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryTotalIntakeDistributionSectionView : SectionView<NonDietaryTotalIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.IntakeDistributionBins.Count > 0) {
                var chartCreator1 = new NonDietaryTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "NonDietaryTotalIntakeDistributionChartCreatorChart",
                        chartCreator1,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator1.Title,
                        true
                    );

                var chartCreator2 = new NonDietaryTotalIntakeCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "NonDietaryTotalIntakeCumulativeDistributionChartCreatorChart",
                        chartCreator2,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator2.Title,
                        true
                    );
            } else {
                sb.AppendParagraph("No non-dietary exposure distribution available");
            }
        }
    }
}
