using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExternalExposureTotalDistributionRouteSectionView : SectionView<ExternalExposureTotalDistributionRouteSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var isUncertainty = Model.Records.Count > 0 &&  Model.Records.First().Contributions.Count > 0;
            var hiddenProperties = new List<string>();
            if (!isUncertainty) {
                hiddenProperties.Add(nameof(ExternalExposureDistributionRouteRecord.LowerContributionPercentage));
                hiddenProperties.Add(nameof(ExternalExposureDistributionRouteRecord.UpperContributionPercentage));
                hiddenProperties.Add(nameof(ExternalExposureDistributionRouteRecord.MeanContribution));
            } else {
                hiddenProperties.Add(nameof(ExternalExposureDistributionRouteRecord.ContributionPercentage));
            }
            //Render HTML
            var chartCreator = new ExternalExposureTotalDistributionRoutePieChartCreator(Model, isUncertainty);
            sb.AppendChart(
                "ExternalExposureTotalDistributionRoutePieChart",
                chartCreator,
                ChartFileType.Svg,
                Model,
                ViewBag,
                chartCreator.Title,
                true
            );
            sb.AppendParagraph("Absorption factors are not used");
            sb.AppendTable(
                Model,
                Model.Records,
                "TotalDistributionExternalExposureRouteTable",
                ViewBag,
                caption: "External exposure contributions to the total distribution for routes",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
