using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class UpperDistributionRouteCompoundSectionView : SectionView<UpperDistributionRouteCompoundSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }
            if (Model.Records.All(r => double.IsNaN(r.ContributionPercentage))) {
                hiddenProperties.Add("ContributionPercentage");
                hiddenProperties.Add("Contribution");
                hiddenProperties.Add("MeanContribution");
                hiddenProperties.Add("RelativePotencyFactor");
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
            }
            //Render HTML
            if (Model.Records.Count > 0) {
                sb.AppendParagraph($"Exposure: upper tail {Model.CalcalatedUpperPercentage:F1}% ({Model.NRecords} records), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

                if (Model.Records.Count > 1) {
                    var chartCreator = new UpperDistributionRouteCompoundPieChartCreator(Model, isUncertainty);
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
                    Model.Records,
                    "UpperDistributionRouteSubstanceTable", 
                    ViewBag, 
                    caption: $"Exposure statistics by route x substance for the upper tail of the distribution, RPFs are not applied except for exposure contribution (estimated {Model.CalcalatedUpperPercentage:F1}%).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendParagraph("No positive exposures found");
            }
        }
    }
}
