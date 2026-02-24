using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalContributionBySourceTotalSectionView : SectionView<ExternalContributionBySourceTotalSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var isUncertainty = Model.Records.First().Contributions.Count > 0;
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(ExternalContributionBySourceRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(ExternalContributionBySourceRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(ExternalContributionBySourceRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(ExternalContributionBySourceRecord.ContributionPercentage));
            }
            if (Model.Records.Count > 1) {
                var chartCreator = new ExternalContributionBySourceTotalPieChartCreator(Model, isUncertainty);
                sb.AppendChart(
                    "ExternalTotalDistributionSourceChart",
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
                "ExternalExposureBySourceTotalTable",
                ViewBag,
                caption: "Contributions by source (total distribution).",
                saveCsv: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
