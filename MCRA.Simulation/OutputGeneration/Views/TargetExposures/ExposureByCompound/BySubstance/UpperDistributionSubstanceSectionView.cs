using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionSubstanceSectionView : SectionView<UpperDistributionSubstanceSection> {
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
            }
            if (Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
            }

            var records = isUncertainty
                ? Model.Records.Where(c => c.MeanContribution > 0).ToList()
                : Model.Records.Where(c => c.Mean > 0).ToList();

            //Render HTML
            if (records.Any()) {
                var description = $"Total {records.Count} substance(s) with positive exposure in the upper tail. "
                    + $"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NumberOfIntakes} records), "
                    + $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, "
                    + $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}.";

                sb.AppendDescriptionParagraph(description);

                if (records.Count > 1) {
                    var chartCreator = new UpperDistributionSubstancePieChartCreator(Model, isUncertainty);
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
                    "UpperDistributionSubstanceTable",
                    ViewBag,
                    caption: $"Exposure statistics by substance to the upper tail of the distribution, RPFs are not applied except for exposure contribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                if (Model.UpperPercentage.HasValue) {
                    sb.AppendParagraph("No positive exposures found.");
                } else {
                    sb.AppendParagraph("Upper distribution can not be determined because no relative potency factors are available.");
                }
            }
        }
    }
}
