using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksExposureThresholdRatioSectionView : SectionView<SingleValueRisksExposureThresholdRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedRisk");
                hiddenProperties.Add("PUpperAdjustedRisk");
                hiddenProperties.Add("PLowerAdjustedRisk");
            }
            var isUncertainty = Model.Records.First().Risks.Count > 0;
            if (isUncertainty) {
                hiddenProperties.Add("AdjustedRisk");
                hiddenProperties.Add("Risk");
                hiddenProperties.Add("ReferenceValueExposure");
            } else {
                hiddenProperties.Add("PUpperAdjustedRisk");
                hiddenProperties.Add("PLowerAdjustedRisk");
                hiddenProperties.Add("PUpperRisk_uncertainty");
                hiddenProperties.Add("PLowerRisk_uncertainty");
                hiddenProperties.Add("MedianAdjustedRisk");
                hiddenProperties.Add("MedianRisk");
                hiddenProperties.Add("MedianExposure");
            }

            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("PUpperRisk");
                hiddenProperties.Add("PLowerRisk");
                hiddenProperties.Add("MedianAdjustedRisk");
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedRisk");
            }
            var description = $"Single value risks based on individual risk distribution. ";
            description += Model.IsInversDistribution ? $"The specified percentile is calculated using the inverse distribution." : $"The exposure percentile is calculated using the inverse distribution.";
            sb.AppendDescriptionParagraph(description);

            if (isUncertainty && Model.UseAdjustmentFactor) {
                var chartCreatorBoxPlot = new SingleValueRisksHIUncertaintyChartCreator(Model);
                sb.AppendChart(
                    "UncertaintyHIChart",
                    chartCreatorBoxPlot,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreatorBoxPlot.Title,
                    true
                );
            }
            sb.AppendDescriptionParagraph("Single value risk estimates: exposure/threshold value.");
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueRisksFromDistributionsHITable",
                ViewBag,
                header: true,
                saveCsv: true,
                rotate: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
