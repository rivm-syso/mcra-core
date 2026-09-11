using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksHazardExposureRatioSectionView : SectionView<SingleValueRisksHazardExposureRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.AdjustmentFactor));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.AdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.PUpperAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.PLowerAdjustedRisk));
            }
            var isUncertainty = Model.Records.First().Risks.Count > 0;
            if (isUncertainty) {
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.AdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.Risk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.ReferenceValueExposure));
            } else {
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.PUpperAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.PLowerAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.PUpperRisk_uncertainty));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.PLowerRisk_uncertainty));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.MedianAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.MedianRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.MedianExposure));
            }
            if (Model.Records.All(r => r.SubstanceCode == null)) {
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.SubstanceCode));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.SubstanceName));
            }
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.MedianAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.AdjustmentFactor));
                hiddenProperties.Add(nameof(SingleValueRisksHazardExposureRatioRecord.AdjustedRisk));
            }

            var description = "Single value risks based on individual risk distribution. ";
            description += Model.IsInversDistribution
                ? "The specified risk percentile is calculated using the inverse distribution."
                : "The exposure percentile is calculated using the inverse distribution.";
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
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueRisksFromDistributionsMOETable",
                ViewBag,
                caption: "Single value risk estimates.",
                header: true,
                saveCsv: true,
                rotate: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
