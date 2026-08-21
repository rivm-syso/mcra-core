using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskContributionsBySubstanceAtRiskSectionView : SectionView<RiskContributionsBySubstanceAtRiskSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (!Model.ContributionRecords.All(c => double.IsNaN(c.Contribution))) {
                var hiddenProperties = new List<string>();

                var isUncertainty = Model.ContributionRecords.FirstOrDefault()?.Contributions.Any() ?? false;
                if (!isUncertainty) {
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.LowerContributionPercentage));
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.UpperContributionPercentage));
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.MeanContribution));
                } else {
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.Contribution));
                }
                hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.MeanAll));
                hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.Stratification));

                //Render HTML
                if (Model.ContributionRecords.Any()) {
                    var chartCreator = new RiskContributionsBySubstanceAtRiskPieChartCreator(Model.SectionId, Model.ContributionRecords, isUncertainty);
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

                    sb.AppendDescriptionParagraph($"Upper distribution {Model.ContributionRecords.Count} substances.");
                    sb.AppendTable(
                        Model,
                        Model.ContributionRecords,
                        "RiskContributionsBySubstanceUpperAtRiskTable",
                        ViewBag,
                        caption: $"Risk contributions by substance of the upper {percentageAtRisk:F1}% of the distribution exceeding the threshold.",
                        saveCsv: true,
                        displayLimit: 10,
                        hiddenProperties: hiddenProperties
                    );

                } else {
                    sb.AppendNotification("No positive exposures.");
                }
            } else {
                sb.AppendNotification("No individuals.");
            }
        }
    }
}
