using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardDistributionSectionView : SectionView<HazardDistributionSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.PercentilesGrid.First().UncertainValues.Count > 0;

            //Render HTML
            sb.AppendParagraph($"Critical Effect Dose Average Human = {Model.CriticalEffectDoseHuman:G4} ({ViewBag.GetUnit("IntakeUnit")})");
            if (!double.IsNaN(Model.UpperVariationFactor)) {
                sb.Append($"<p>A p95-sensitive person is {Model.LowerVariationFactor}-{Model.UpperVariationFactor} " +
                       "times as sensitive as an average person. This corresponds with a geometric standard deviation " +
                      $"(intraspecies GSD) = {Model.GeometricStandardDeviation:G3}</p>");
                if (isUncertainty) {
                    sb.AppendParagraph($"with uncertainty characterised by df = {Model.DegreesOfFreedom:G3}");
                }
            }
            sb.Append("<div class=\"figure-container\">");
            var chartCreator = new HazardDistributionChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "HazardIndexDistributionChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );

            var chartCreator1 = new HazardDistributionCumulativeChartCreator(Model, ViewBag.GetUnit("IntakeUnit"));
            sb.AppendChart(
                "HazardIndexCumulativeChart",
                chartCreator1,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator1.Title,
                true
            );
            sb.Append("</div>");
        }
    }
}
