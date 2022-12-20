using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class NonDietaryUpperIntakeDistributionSectionView : SectionView<NonDietaryUpperIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            if (Model.IntakeDistributionBins.Count > 0) {
                var chartCreator = new NonDietaryUpperIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "NonDietaryUpperIntakeDistributionChartCreatorChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            } else {
                sb.AppendParagraph("No nondietary upper exposure distribution available");
            }
        }
    }
}
