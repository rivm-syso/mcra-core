using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalUpperExposureDistributionSectionView : SectionView<ExternalUpperExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            if (Model.IntakeDistributionBins?.Count > 0) {
                var chartCreator = new ExternalUpperExposureDistributionChartCreator(Model, ViewBag.GetUnit("ExternalExposureUnit"));
                sb.AppendChart(
                    "ExternalUpperExposureDistributionChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            } else {
                sb.AppendParagraph("No external upper exposure distribution available.");
            }
        }
    }
}
