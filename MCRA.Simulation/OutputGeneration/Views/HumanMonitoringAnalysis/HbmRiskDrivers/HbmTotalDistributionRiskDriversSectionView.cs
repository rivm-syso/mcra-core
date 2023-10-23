using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class HbmTotalDistributionRiskDriversSectionView : SectionView<HbmTotalDistributionRiskDriversSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = false;
            if (Model.Records.All(c => double.IsNaN(c.LowerContributionPercentage))) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("Contribution");
                isUncertainty = true;
            }

            //Render HTML
            if (Model.Records.Count > 1) {
                var chartCreator = new HbmTotalDistributionRiskDriversPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "HbmTotalDistributionRiskDriversChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );
                sb.AppendDescriptionParagraph($"Number of substances: {Model.Records.Count}");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "HbmTotalDistributionRiskDriversTable",
                    ViewBag,
                    caption: "Risk drivers for substances (total distribution).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendDescriptionParagraph("No positive concentrations found");
            }
        }
    }
}
