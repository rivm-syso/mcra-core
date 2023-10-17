using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmUpperDistributionRiskDriversSectionView : SectionView<HbmUpperDistributionRiskDriversSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var result = new List<HbmRiskDriverRecord>();
            if (Model.Records.All(c => double.IsNaN(c.UncertaintyLowerBound))) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
                result = Model.Records.Where(c => c.Contribution > 0).ToList();
            } else {
                hiddenProperties.Add("Contribution");
                result = Model.Records.Where(c => c.Contribution > 0).ToList();
            }
            
            //Render HTML
            sb.Append($"Exposure: upper percentage {Model.UpperPercentage:F2} % ({Model.NumberOfIntakes} records), " +
                               $"minimum value {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                               $"maximum value {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");
            if (result.Count > 1) {
                var chartCreator = new HbmUpperDistributionRiskDriversPieChartCreator(Model, false);
                sb.AppendChart(
                    "HbmUpperDistributionRiskDriversChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.AppendDescriptionParagraph($"Number of substances: {result.Count}");
                sb.AppendTable(
                   Model,
                   result,
                   "UpperDistributionRiskDriversTable",
                   ViewBag,
                   caption: "Risk drivers for substances (upper distribution).",
                   saveCsv: true,
                   header: true,
                   hiddenProperties: hiddenProperties
                );
            } else {
                sb.Append("No positive concentrations found");
            }
        }
    }
}
