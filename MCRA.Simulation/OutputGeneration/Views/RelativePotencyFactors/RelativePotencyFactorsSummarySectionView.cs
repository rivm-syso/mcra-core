using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RelativePotencyFactorsSummarySectionView : SectionView<RelativePotencyFactorsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => double.IsNaN(r.RelativePotencyFactorLowerBoundPercentile))) {
                hiddenProperties.Add("RelativePotencyFactorLowerBoundPercentile");
                hiddenProperties.Add("RelativePotencyFactorUpperBoundPercentile");
            }
            var failedRecordCount = Model.Records.Where(r => double.IsNaN(r.RelativePotencyFactor)).Count();

            //Render HTML
            if (Model.Records.Select(c => c.CompoundName).Distinct().Count() > 1) {
                if (failedRecordCount > 0) {
                    sb.AppendParagraph($"Error: failed to compute the relative potency factor for {failedRecordCount} substances", "warning");
                }
                if (Model.Records.Any(r => !double.IsNaN(r.RelativePotencyFactor))) {
                    var chartCreator = new RelativePotencyFactorsChartCreator(Model);
                    sb.AppendChart(
                        "RelativePotencyFactorsChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                sb.AppendDescriptionParagraph($"Number of relative potency factors: {Model.Records.Count}");
                sb.AppendTable(
                   Model,
                   Model.Records,
                   "RelativePotencyFactorsTable",
                   ViewBag,
                   caption: "Relative potency factors by substance.",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No info available");
            }
        }
    }
}
