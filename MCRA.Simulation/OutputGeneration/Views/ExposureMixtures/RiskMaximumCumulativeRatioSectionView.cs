using System.Text;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class RiskMaximumCumulativeRatioSectionView : SectionView<RiskMaximumCumulativeRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var individualDayUnits = "individuals";
            var individualDayUnit = "individual";
            if (ViewBag.GetUnit("IndividualDayUnit") != individualDayUnits) {
                individualDayUnit = "individual day";
            }

            //Render HTML
            var riskMetricType = Model.RiskMetricType == RiskMetricType.ExposureHazardRatio
                ? "Risk is defined as exposure/hazard (E/H)."
                : "Risk is defined as hazard/exposure (H/E).";

            sb.AppendDescriptionParagraph($"Maximum Cumulative Ratio (MCR) plot: total risk / maximum risk vs total risk (n = {Model.DriverSubstanceTargets.Count}). {riskMetricType}");
            sb.AppendDescriptionParagraph($"Each dot represents the MCR of an {individualDayUnit} and is calculated as the sum of risk characterisation ratios " +
                $"divided by the ratio of the highest contributing substance.");
            if (Model.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                sb.AppendDescriptionParagraph($"The blue area, where E/H < threshold, marks combined exposures that do not present any concern.");
                sb.AppendDescriptionParagraph($"The white area - defined by the vertical red line for acceptable risks and the curved line " +
                    $" depicting E/H = threshold * MCR (with default threshold of 1) - shows subjects with combined exposures above the threshold (E/H > threshold) " +
                    $" but whose risk quotient for any single substance does not exceed the limit.");
                sb.AppendDescriptionParagraph($"The red area represents subjects where E/H > threshold and where exposures produces risk " +
                    $"quotients exceeding the threshold for at least one substance.");
            } else {
                sb.AppendDescriptionParagraph($"The blue area, where H/E > threshold, marks combined exposures that do not present any concern.");
                sb.AppendDescriptionParagraph($"The white area defined - by the vertical red line for acceptable risks and the curved line " +
                    $" depicting H/E = threshold/MCR (with default threshold of 1) - shows subjects with combined exposures below the threshold (H/E < threshold) " +
                    $" but  whose risk quotient for any single substance does not exceed the limit.");
                sb.AppendDescriptionParagraph($"The red area represents subjects where H/E < threshold/MCR and where exposures produces risk " +
                    $"quotients exceeding the threshold for at least one substance.");
            }
            sb.AppendDescriptionParagraph($"Data points below the horizontal red line (corresponding to MCR = 2) indicate subjects where one substance " +
                $"contributes 50% ore more to the total risk (e.g. single substance issue). Above the line, subjects experience combined exposures from multiple " +
                $"substances (e.g. co-exposure).");

            if (Model.DriverSubstanceTargets.Count > 1) {
                if (!Model.SkipPrivacySensitiveOutputs) {
                    var chartCreator = new MCRChartCreator(Model);
                    sb.AppendChart(
                        "DriverSubstancesTotalChart",
                        chartCreator,
                        ChartFileType.Svg,
                        Model,
                        ViewBag,
                        chartCreator.Title,
                        true
                    );
                }
            } else {
                sb.AppendDescriptionParagraph("No MCR graph available (too few observations).");
            }
        }
    }
}
