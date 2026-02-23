using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskContributionsBySubstanceUpperSectionView : SectionView<RiskContributionsBySubstanceUpperSection> {
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
                    var chartCreator = new RiskContributionsBySubstanceUpperPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "RiskContributionsBySubstanceUpperChart",
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
                        "RiskContributionsBySubstanceUpperTable",
                        ViewBag,
                        caption: $"Risk contributions by substance upper distribution (estimated {percentageAtRisk:F1}%).",
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
