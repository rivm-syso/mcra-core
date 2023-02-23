using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksMarginOfExposureSectionView : SectionView<SingleValueRisksMarginOfExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedMarginOfExposure");
                hiddenProperties.Add("PUpperAdjustedMarginOfExposure");
                hiddenProperties.Add("PLowerAdjustedMarginOfExposure");
            }
            var isUncertainty = Model.Records.First().MarginOfExposures.Count > 0;
            if (isUncertainty) {
                hiddenProperties.Add("AdjustedMarginOfExposure");
                hiddenProperties.Add("MarginOfExposure");
                hiddenProperties.Add("ReferenceValueExposure");
            } else {
                hiddenProperties.Add("PUpperAdjustedMarginOfExposure");
                hiddenProperties.Add("PLowerAdjustedMarginOfExposure");
                hiddenProperties.Add("PUpperMOE_uncertainty");
                hiddenProperties.Add("PLowerMOE_uncertainty");
                hiddenProperties.Add("MedianAdjustedMarginOfExposure");
                hiddenProperties.Add("MedianMarginOfExposure");
                hiddenProperties.Add("MedianExposure");
            }
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("PUpperMarginOfExposure");
                hiddenProperties.Add("PLowerMarginOfExposure");
                hiddenProperties.Add("AdjustedMarginOfExposure");
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedMarginOfExposure");
            }


            var description = $"Single value risks based on individual risk distribution. ";
            description += Model.IsInversDistribution ? $"The specified MOE percentile is calculated using the inverse distribution." : $"The exposure percentile is calculated using the inverse distribution.";
            sb.AppendDescriptionParagraph(description);
            if (isUncertainty && Model.UseAdjustmentFactor) {
                var chartCreatorBoxPlot = new SingleValueRisksMOEUncertaintyChartCreator(Model);
                sb.AppendChart(
                    "UncertaintyMOEChart",
                    chartCreatorBoxPlot,
                    ChartFileType.Svg,
                    Model,
                    ViewBag,
                    chartCreatorBoxPlot.Title,
                    true
                );
            }
            sb.AppendDescriptionParagraph("Single value risk estimates: margin of exposure.");
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueRisksFromDistributionsMOETable",
                ViewBag,
                header: true,
                saveCsv: true,
                rotate: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
