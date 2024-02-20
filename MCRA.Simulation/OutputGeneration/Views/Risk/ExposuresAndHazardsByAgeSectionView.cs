using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {

    public class ExposuresAndHazardsByAgeSectionView : SectionView<ExposuresAndHazardsByAgeSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (Model.HazardExposureByAgeRecords.Any(r => r.Age.HasValue && r.Exposure > 0)) {
                var chartCreator = new ExposuresAndHazardsByAgeChartCreator(Model);
                sb.AppendChart(
                    "ExposuresAndHazardsByAgeChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            }
        }
    }
}
