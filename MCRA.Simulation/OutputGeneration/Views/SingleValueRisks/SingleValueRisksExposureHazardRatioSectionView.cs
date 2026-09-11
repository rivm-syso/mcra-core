using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksExposureHazardRatioSectionView : SectionView<SingleValueRisksExposureHazardRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.AdjustmentFactor));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.AdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.PUpperAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.PLowerAdjustedRisk));
            }
            var isUncertainty = Model.Records.First().Risks.Count > 0;
            if (isUncertainty) {
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.AdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.Risk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.ReferenceValueExposure));
            } else {
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.PUpperAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.PLowerAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.PUpperRisk_uncertainty));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.PLowerRisk_uncertainty));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.MedianAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.MedianRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.MedianExposure));
            }
            if (Model.Records.All(r => r.SubstanceCode == null)) {
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.SubstanceCode));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.SubstanceName));
            }
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.MedianAdjustedRisk));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.AdjustmentFactor));
                hiddenProperties.Add(nameof(SingleValueRisksExposureHazardRatioRecord.AdjustedRisk));
            }

            var description = "Single value risks based on individual risk distribution. ";
            description += Model.IsInversDistribution
                ? "The specified risk percentile is calculated using the inverse distribution."
                : "The exposure percentile is calculated using the inverse distribution.";
            sb.AppendDescriptionParagraph(description);

            if (isUncertainty && Model.UseAdjustmentFactor) {
                var chartCreatorBoxPlot = new SingleValueRisksExposureHazardRatioUncertaintyChartCreator(Model);
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
            sb.AppendTable(
                Model,
                Model.Records,
                "SingleValueRisksFromDistributionsHITable",
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
