using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskContributionsBySubstanceSectionView : SectionView<RiskContributionsBySubstanceSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();

            var isUncertainty = Model.Records.FirstOrDefault()?.Contributions.Any() ?? false;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(RiskBySubstanceRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(RiskBySubstanceRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(RiskBySubstanceRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(RiskBySubstanceRecord.ContributionPercentage));
            }

            //Render HTML
            if (Model.Records.Any()) {
                var chartCreator = new RiskContributionsBySubstancePieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "RiskContributionsBySubstanceTotalChart",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreator.Title,
                    true
                );

                sb.AppendDescriptionParagraph($"Total distribution {Model.Records.Count} substances.");
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "RiskContributionsBySubstanceTotalTable",
                    ViewBag,
                    caption: $"Risk contributions by substance (total distribution).",
                    saveCsv: true,
                    displayLimit: 10,
                    hiddenProperties: hiddenProperties
                );

            } else {
                sb.AppendNotification("No positive exposures.");
            }
        }
    }
}
