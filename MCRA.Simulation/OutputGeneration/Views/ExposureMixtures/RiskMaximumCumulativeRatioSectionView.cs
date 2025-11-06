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
            sb.AppendDescriptionParagraph($"For each {individualDayUnit} the MCR is calculated as the sum of risk characterisation ratios and " +
                $"divided by the ratio of the highest contributing substance.");
            if (Model.RiskMetricType == RiskMetricType.ExposureHazardRatio) {
                sb.AppendDescriptionParagraph($"The blue area, where E/H < threshold, marks combined exposures that do not present any concerns.");
                sb.AppendDescriptionParagraph($"The white area defined by the vertical red line for acceptable risks and the curved line " +
                    $" depicting E/H = threshold * MCR (with default value threshold = 1) shows subjects with combined exposures above the threshold (E/H > threshold) " +
                    $" but without exceeding the risk quotient for any single substance.");
                sb.AppendDescriptionParagraph($"The red area is for subjects where E/H > threshold and with exposures producing risk " +
                    $"quotients exceeding the threshold for at least one substance.");
            } else {
                sb.AppendDescriptionParagraph($"The blue area, where H/E > threshold, marks combined exposures that do not present any concerns.");
                sb.AppendDescriptionParagraph($"The white area defined by the vertical red line for acceptable risks and the curved line " +
                    $" depicting H/E = threshold/MCR (with default value threshold = 1) shows subjects with combined exposures below the threshold (H/E < threshold) " +
                    $" but without exceeding the risk quotient for any single substance.");
                sb.AppendDescriptionParagraph($"The red area is for subjects where H/E < threshold/MCR and with exposures producing risk " +
                    $"quotients exceeding the threshold for at least one substance.");
            }
            sb.AppendDescriptionParagraph($"Data points below the horizontal red line corresponding to MCR = 2 show subjects in whom one substance " +
                $"contributed 50% ore more to the risk (e.g. single substance issue). Above the line, subjects experience combined exposures from multiple " +
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
