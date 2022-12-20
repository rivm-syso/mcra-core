using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
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
            }
            if (Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
            }

            var records = isUncertainty
                ? Model.Records.Where(c => c.MeanContribution > 0).Select(c => c).ToList()
                : Model.Records.Where(c => c.Mean > 0).Select(c => c).ToList();

            if (records.Any()) {
                var description = $"Total {records.Count} substance(s) with positive exposure.";
                sb.AppendDescriptionParagraph(description);
                // Contributions pie chart
                if (records.Where(r => !double.IsNaN(r.ContributionPercentage)).Count() > 1) {
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

                // Exposures by substance table
                sb.AppendTable(
                    Model,
                    records,
                    "TotalDistributionCompoundTable",
                    ViewBag,
                    caption: "Exposure statistics by substance (total distribution), RPFs are not applied except for exposure contribution.",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No positive exposures found.");
            }
        }
    }
}
