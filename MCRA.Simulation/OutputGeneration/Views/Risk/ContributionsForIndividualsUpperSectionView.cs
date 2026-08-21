using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ContributionsForIndividualsUpperSectionView : SectionView<ContributionsForIndividualsUpperSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            if (!Model.IndividualContributionRecords.All(c => double.IsNaN(c.Contribution))) {
                var isUncertainty = Model.IndividualContributionRecords.FirstOrDefault()?.Contributions.Any() ?? false;
                var hiddenProperties = new List<string>() {
                    nameof(RiskContributionBySubstanceRecord.PositivesCount),
                    nameof(RiskContributionBySubstanceRecord.PercentagePositives),
                    nameof(RiskContributionBySubstanceRecord.MeanAll),
                    nameof(RiskContributionBySubstanceRecord.Stratification),
                };
                if (Model.IndividualContributionRecords.All(c => double.IsNaN(c.LowerContributionPercentage))) {
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.LowerContributionPercentage));
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.UpperContributionPercentage));
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.MeanContribution));
                } else {
                    hiddenProperties.Add(nameof(RiskContributionBySubstanceRecord.Contribution));
                }

                sb.Append(TableHelpers.CsvExportLink(
                    "IndividualContributionsUpperBoxPlotTable", Model, Model.BoxPlotRecords, ViewBag, true, true)
                );

                sb.Append("<div class=\"figure-container\">");
                var chartCreator = new IndividualContributionsUpperBySubstanceBoxPlotChartCreator(
                    Model,
                    Model.ShowOutliers
                );
                sb.AppendChart(
                    "SubstanceContributionsIndividualRisksUpperBoxPlot",
                    chartCreator,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreator.Title,
                    saveChartFile: true
                );
                var chartCreatorPie = new IndividualContributionsUpperPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "SubstanceContributionsIndividualRisksUpperPieChart",
                    chartCreatorPie,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    caption: chartCreatorPie.Title,
                    saveChartFile: true
                );
                sb.Append("</div>");

                var percentageAtRisk = Model.PercentagesAtRisk.Percentages.Any()
                    ? Model.PercentagesAtRisk.MedianContribution
                    : Model.PercentagesAtRisk.Percentage;

                sb.AppendTable(
                    Model,
                    Model.IndividualContributionRecords,
                    "SubstanceContributionsIndividualUpperisksTable",
                    ViewBag,
                    displayLimit: 10,
                    caption: $"Mean contributions to risk for individuals for upper distribution (estimated {percentageAtRisk:F1}%).",
                    saveCsv: true,
                    hiddenProperties: hiddenProperties
                );
            } else {
                sb.AppendNotification("No individuals in upper.");
            }
        }
    }
}
