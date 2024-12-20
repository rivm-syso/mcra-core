using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalTotalExposureDistributionSectionView : SectionView<ExternalTotalExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            //Render HTML
            if (Model.IntakeDistributionBins.Count > 0) {
                var chartCreator1 = new ExternalTotalIntakeDistributionChartCreator(Model, ViewBag.GetUnit("ExternalExposureUnit"));
                sb.AppendChart(
                        "ExternalTotalExposureDistributionChartCreatorChart",
                        chartCreator1,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator1.Title,
                        true
                    );

                var chartCreator2 = new ExternalTotalExposureCumulativeDistributionChartCreator(Model, ViewBag.GetUnit("ExternalExposureUnit"));
                sb.AppendChart(
                        "ExternalTotalExposureCumulativeDistributionChartCreatorChart",
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
