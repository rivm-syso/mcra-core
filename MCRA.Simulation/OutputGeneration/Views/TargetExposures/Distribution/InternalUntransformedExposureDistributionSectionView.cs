using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class InternalUntransformedExposureDistributionSectionView : SectionView<InternalUntransformedExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.PercentageZeroIntake < 100) {

                var chartCreator = new InternalUntransformedExposureDistributionSectionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "UntransformedExposureDistributionChart",
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
