using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureHazardRatioSubstanceUpperSectionView : SectionView<ExposureHazardRatioSubstanceUpperSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenProperties = new List<string>();

            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add("LowerContributionPercentage");
                hiddenProperties.Add("UpperContributionPercentage");
                hiddenProperties.Add("MeanContribution");
            } else {
                hiddenProperties.Add("ContributionPercentage");
            }

            //Render HTML
            if (Model.Records.Any()) {
                var chartCreator = new DistributionRiskEHDriversPieChartCreator(null, Model, isUncertainty);
                sb.AppendChart(
                    "UpperDistributionRiskEHDriversChart",
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
                    "UpperExposureHazardBySubstanceTable",
                    ViewBag,
                    caption: $"Risk statistics by substance upper distribution (estimated {Model.CalculatedUpperPercentage.ToString("F1")}%).",
                    saveCsv: true,
                    displayLimit: 20,
                    hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendParagraph("No positive exposures found", "warning");
            }
        }
    }
}
