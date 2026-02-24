using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class TotalDistributionCompoundSectionView : SectionView<TotalDistributionCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Any();

            if (!isUncertainty) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(FoodAsMeasuredSubstanceProcessingTypeRecord.ContributionPercentage));
            }
            if (Model.Records.All(c => c.AssessmentGroupMembership == 1)) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.AssessmentGroupMembership));
            }
            if (Model.Records.All(c => double.IsNaN(c.RelativePotencyFactor))) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.RelativePotencyFactor));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.ContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.Contribution));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.MeanContribution));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.UpperContributionPercentage));
            }

            var records = Model.Records.Where(c => c.Mean > 0).ToList();
            if (records.Count > 0) {
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
            }

            records = Model.Records.Where(c => c.Mean > 0 || c.MeanContribution > 0).ToList();
            if (records.Count > 0) {
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
                sb.AppendNotification("No positive exposures.");
            }
        }
    }
}
