using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalUpperExposureDistributionSectionView : SectionView<ExternalUpperExposureDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            if (Model.IntakeDistributionBins.Count > 0) {
                var chartCreator = new ExternalUpperExposureDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
                sb.AppendChart(
                        "ExternalUpperExposureDistributionChartCreatorChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
            } else {
                sb.AppendParagraph("No external upper exposure distribution available");
            }
        }
    }
}
