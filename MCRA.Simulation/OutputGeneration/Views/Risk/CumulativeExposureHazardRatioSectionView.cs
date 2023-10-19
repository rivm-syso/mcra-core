using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class CumulativeExposureHazardRatioSectionView : SectionView<CumulativeExposureHazardRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.RiskRecords
                .Any(c => c.Records.First().RiskPercentiles[0].UncertainValues?.Any() ?? false);
            var hiddenProperties = new List<string>();
            if (Model.RiskRecords.First().Records.All(c => c.BiologicalMatrix == string.Empty)) {
                hiddenProperties.Add("BiologicalMatrix");
                hiddenProperties.Add("ExpressionType");
            }
            var targets = Model.RiskRecords.Select(c => c.Target).ToList();
            foreach (var target in targets) {
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
}
