using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionByRouteSubstanceUpperSectionView : SectionView<ContributionByRouteSubstanceUpperSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }

            var records = Model.Records.Where(r => !double.IsNaN(r.ContributionPercentage)).ToList();

            if (records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }
            if (records.Count > 0) {
                var individualString = Model.NumberOfIntakes == 1 ? $"1 individual" : $"{Model.NumberOfIntakes} individuals";
                sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({individualString}), " +
                    $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                    $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

                if (records.Count > 1) {
                    var chartCreator = new ContributionByRouteSubstanceUpperPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "UpperDistributionRouteSubstanceChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                sb.AppendTable(
                    Model,
                    records,
                    "UpperDistributionRouteSubstanceTable",
                    ViewBag,
                    caption: $"Contributions by route and substance for the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                if (Model.UpperPercentage.HasValue) {
                    sb.AppendNotification("No positive exposures.");
                } else {
                    sb.AppendParagraph("Upper distribution can not be determined because no relative potency factors are available.");
                }
            }
        }
    }
}
