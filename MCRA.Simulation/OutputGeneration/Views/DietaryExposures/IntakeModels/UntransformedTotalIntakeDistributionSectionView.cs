using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UntransformedTotalIntakeDistributionSectionView : SectionView<UntransformedTotalIntakeDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.PercentageZeroIntake < 100) {

                var chartCreator = new UntransformedTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "UntransformedTotalIntakeDistributionChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            } else {
                sb.AppendDescriptionParagraph("No positive exposures.");
            }
        }
    }
}
