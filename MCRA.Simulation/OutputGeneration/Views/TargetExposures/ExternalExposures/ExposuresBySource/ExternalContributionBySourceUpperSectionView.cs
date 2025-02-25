using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalContributionBySourceUpperSectionView : SectionView<ExternalContributionBySourceUpperSection> {
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
            sb.AppendParagraph($"Exposure: upper tail {Model.CalculatedUpperPercentage:F1}% ({Model.NumberOfIntakes} records), " +
                $"minimum {Model.LowPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}, " +
                $"maximum {Model.HighPercentileValue:G4} {ViewBag.GetUnit("IntakeUnit")}");

            if (Model.Records.Count > 1) {
                var chartCreator = new ExternalContributionsBySourceUpperPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "UpperDistributionSourceChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
            } else {
                sb.AppendParagraph("No upper distribution available for specified percentage");
            }
            sb.AppendTable(
                Model,
                Model.Records,
                "ExternalExposureBySourceUpperTable",
                ViewBag,
                caption: $"Contributions by source for the upper distribution (estimated {Model.CalculatedUpperPercentage:F1}%).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
