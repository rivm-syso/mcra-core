using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionCompoundSectionView : SectionView<TotalDistributionCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Any();

            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            if (Model.Records.All(c => c.AssessmentGroupMembership == 1)) {
                hiddenProperties.Add("AssessmentGroupMembership");
            }
            if (Model.Records.All(c => double.IsNaN(c.RelativePotencyFactor))) {
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }

            var records = Model.Records.Where(c => c.Mean > 0).ToList();

            if (records.Any()) {
                var description = $"Total {records.Count} substance(s) with positive exposure.";
                sb.AppendDescriptionParagraph(description);
                // Contributions pie chart
                if (records.Count(r => !double.IsNaN(r.ContributionPercentage)) > 1) {
                    var chartCreator = new TotalDistributionCompoundPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "TotalDistributionSubstanceChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
                var rpfMessage = Model.Records.All(c => double.IsNaN(c.Contribution)) ? string.Empty : " RPFs are not applied except for exposure contribution.";
                var caption = $"Exposure statistics by substance (total distribution).{rpfMessage}";
                caption = Model.ExposureTarget == null ? caption : $"{caption} Biological matrix: {Model.ExposureTarget}";
                // Exposures by substance table
                sb.AppendTable(
                    Model,
                    records,
                    "TotalDistributionCompoundTable",
                    ViewBag,
                    caption: caption,
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No positive exposures found.");
            }
        }
    }
}
