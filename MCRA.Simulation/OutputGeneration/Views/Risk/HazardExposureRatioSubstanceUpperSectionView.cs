using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HazardExposureRatioSubstanceUpperSectionView : SectionView<HazardExposureRatioSubstanceUpperSection> {
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
                    var chartCreator = new DistributionRiskHEDriversPieChartCreator(null, Model, isUncertainty);
                    sb.AppendChart(
                        "RiskContributionsBySubstanceUpperChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );

                    sb.AppendDescriptionParagraph($"Upper distribution {Model.Records.Count} substances.");
                    sb.AppendTable(
                        Model,
                        Model.Records,
                        "RiskContributionsBySubstanceUpperTable",
                        ViewBag,
                        caption: $"Risk contributions by substance upper distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
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
