using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CumulativeExposureHazardRatioSectionView : SectionView<CumulativeExposureHazardRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.RiskRecords
                .Any(c => c.RiskPercentiles[0].UncertainValues?.Any() ?? false);

            sb.Append("<div class=\"figure-container\">");
            sb.AppendChart(
                name: "CumulativeHazardIndexBySubstanceMedianChart",
                chartCreator: new CumulativeExposureHazardRatioMedianChartCreator(Model, isUncertainty),
                fileType: ChartFileType.Svg,
                section: Model,
                viewBag: ViewBag,
                caption: "Cumulative risk (median) in the population.",
                saveChartFile: true
            );

            sb.AppendChart(
                name: "CumulativeHazardIndexBySubstanceUpperChart",
                chartCreator: new CumulativeExposureHazardRatioUpperChartCreator(Model, isUncertainty),
                fileType: ChartFileType.Svg,
                section: Model,
                viewBag: ViewBag,
                caption: "Cumulative risk (upper) in the population.",
                saveChartFile: true
            );
            sb.Append("</div>");
        }
    }
}
