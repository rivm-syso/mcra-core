using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskContributionsBySubstanceAtRiskSectionView : SectionView<RiskContributionsBySubstanceAtRiskSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (!Model.Records.All(c => double.IsNaN(c.Contribution))) {
                var hiddenProperties = new List<string>();

                var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
                if (!isUncertainty) {
                    hiddenProperties.Add("LowerContributionPercentage");
                    hiddenProperties.Add("UpperContributionPercentage");
                    hiddenProperties.Add("MeanContribution");
                } else {
                    hiddenProperties.Add("ContributionPercentage");
                }
                hiddenProperties.Add("MeanAll");

                //Render HTML
                if (Model.Records.Any()) {
                    var chartCreator = new RiskContributionsBySubstanceAtRiskPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "RiskContributionsBySubstanceUpperAtRiskChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                    var percentageAtRisk = Model.PercentagesAtRisk.Percentages.Any()
                        ? Model.PercentagesAtRisk.MedianContribution
                        : Model.PercentagesAtRisk.Percentage;

                    sb.AppendDescriptionParagraph($"Upper distribution {Model.Records.Count} substances.");
                    sb.AppendTable(
                        Model,
                        Model.Records,
                        "RiskContributionsBySubstanceUpperAtRiskTable",
                        ViewBag,
                        caption: $"Risk contributions by substance of the upper {percentageAtRisk:F1}% of the distribution exceeding the threshold.",
                        saveCsv: true,
                        displayLimit: 10,
                        hiddenProperties: hiddenProperties
                    );

                } else {
                    sb.AppendParagraph("No positive exposures found", "warning");
                }
            } else {
                sb.AppendNotification("No individuals.");
            }
        }
    }
}
