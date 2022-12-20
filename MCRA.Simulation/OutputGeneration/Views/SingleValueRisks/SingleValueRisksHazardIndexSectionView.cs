using MCRA.Simulation.OutputGeneration.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SingleValueRisksHazardIndexSectionView : SectionView<SingleValueRisksHazardIndexSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedHazardIndex");
                hiddenProperties.Add("PUpperAdjustedHazardIndex");
                hiddenProperties.Add("PLowerAdjustedHazardIndex");
            }
            var isUncertainty = Model.Records.First().HazardIndices.Count > 0;
            if (isUncertainty) {
                hiddenProperties.Add("AdjustedHazardIndex");
                hiddenProperties.Add("HazardIndex");
                hiddenProperties.Add("ReferenceValueExposure");
            } else {
                hiddenProperties.Add("PUpperAdjustedHazardIndex");
                hiddenProperties.Add("PLowerAdjustedHazardIndex");
                hiddenProperties.Add("PUpperHI_uncertainty");
                hiddenProperties.Add("PLowerHI_uncertainty");
                hiddenProperties.Add("MedianAdjustedHazardIndex");
                hiddenProperties.Add("MedianHazardIndex");
                hiddenProperties.Add("MedianExposure");
            }

            if (!Model.UseAdjustmentFactor) {
                hiddenProperties.Add("PUpperHazardIndex");
                hiddenProperties.Add("PLowerHazardIndex");
                hiddenProperties.Add("MedianAdjustedHazardIndex");
                hiddenProperties.Add("AdjustmentFactor");
                hiddenProperties.Add("AdjustedHazardIndex");
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
            sb.AppendDescriptionParagraph("Single value risk estimates: hazard index.");
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
