using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksHazardExposureRatioSectionView : SectionView<SingleValueRisksHazardExposureRatioSection> {
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
            if (Model.Records.All(r => r.SubstanceCode == null)) {
                hiddenProperties.Add("SubstanceCode");
                hiddenProperties.Add("SubstanceName");
            }
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("PUpperRisk");
                hiddenProperties.Add("PLowerRisk");
                hiddenProperties.Add("AdjustedRisk");
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedRisk");
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
