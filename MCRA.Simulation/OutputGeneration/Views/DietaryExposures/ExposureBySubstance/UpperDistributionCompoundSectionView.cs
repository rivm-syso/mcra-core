using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionCompoundSectionView : SectionView<UpperDistributionCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var isUncertainty = Model.Records.Any() && Model.Records.First().Contributions.Any();

            if (!isUncertainty) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.ContributionPercentage));
            }
            if (Model.Records.All(c => c.AssessmentGroupMembership == 1)) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.AssessmentGroupMembership));
            }
            if (Model.Records.All(c => double.IsNaN(c.RelativePotencyFactor))) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.RelativePotencyFactor));
            }
            if (Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add(nameof(DistributionCompoundRecord.ContributionPercentage));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.Contribution));
                hiddenProperties.Add(nameof(DistributionCompoundRecord.MeanContribution));
            }

            var records = isUncertainty
                ? Model.Records.Where(c => c.MeanContribution > 0).ToList()
                : [.. Model.Records.Where(c => c.Mean > 0)];

            //Render HTML
            if (records.Any()) {
                var description = $"Total {records.Count} substance(s) with positive exposure in the upper tail. "
                    + $"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NumberOfIntakes} records), "
                    + $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, "
                    + $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}.";

                sb.AppendDescriptionParagraph(description);

                if (records.Count > 1) {
                    var chartCreator = new UpperDistributionCompoundPieChartCreator(Model, isUncertainty);
                    sb.AppendChart(
                        "UpperDistributionSubstanceChart",
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
                    "UpperDistributionCompoundTable",
                    ViewBag,
                    caption: $"Exposure statistics by substance to the upper tail of the distribution (estimated {Model.CalculatedUpperPercentage:F1}%), RPFs are not applied except for exposure contribution.",
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
