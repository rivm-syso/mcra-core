using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class MaximumCumulativeRatioSectionView : SectionView<MaximumCumulativeRatioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var individualDayUnits = "individuals";
            var individualDayUnit = "individual";
            if (ViewBag.GetUnit("IndividualDayUnit") != individualDayUnits) {
                individualDayUnit = "individual day";
                individualDayUnits = "individual days";
            }

            var result = Model.DriverCompoundStatisticsRecords.OrderByDescending(c => c.CumulativeExposureMedian).ToList();
            //Render HTML
            var description = string.Empty;
            if (Model.RiskBased) {
                sb.AppendDescriptionParagraph($"Maximum Cumulative Ratio (MCR) plot: total exposure / maximum exposure vs total exposure (n = {Model.DriverCompounds.Count}). Exposures are expressed in equivalents of the reference substance.");
            } else {
                sb.AppendDescriptionParagraph($"Maximum Cumulative Ratio (MCR) plot: total exposure / maximum exposure vs total exposure (n = {Model.DriverCompounds.Count}).");
            }
            sb.AppendDescriptionParagraph($"For each {individualDayUnit} the exposure is cumulated to a total exposure and divided by the exposure of the highest contributing substance (MCR).");
            sb.AppendDescriptionParagraph($"Ratios above 1 indicate co-exposure, {individualDayUnits} have different colors according to the highest contributing substances.");

            if (Model.IsNewImplementation) {
                sb.AppendDescriptionParagraph($"The black lines represent the regression lines MCR vs ln(Cumulative exposure) for each tail.");
                if (Model.MinimumPercentage > 0) {
                    sb.AppendDescriptionParagraph($"Substances with a contribution less than {Model.MinimumPercentage}% are not displayed.");
                }
                if (Model.DriverCompounds.Count > 1) {
                    sb.Append("<div class=\"figure-container\">");
                    var chartCreator1 = new DriverSubstancesChartCreator(Model);
                    sb.AppendChart(
                            "DriverSubstancesTotalChart",
                            chartCreator1,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator1.Title,
                            true
                        );

                    chartCreator1 = new DriverSubstancesChartCreator(Model, Model.Percentiles.Min());
                    sb.AppendChart(
                            "DriverSubstancesUpperChart",
                            chartCreator1,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator1.Title,
                            true
                        );
                    sb.Append("</div>");
                } else {
                    sb.AppendDescriptionParagraph("No MCR graph available (too few observations).");
                }
                if (Model.MinimumPercentage > 0) {
                    sb.AppendDescriptionParagraph($"Substances with a contribution less than {Model.MinimumPercentage}% are not displayed.");
                }
                sb.AppendDescriptionParagraph($"Summary of MCR contributions to tail.");
                sb.AppendTable(
                   Model,
                   Model.MCRDrilldownRecords,
                   "MCRTable",
                   ViewBag,
                   caption: "Maximum cumulative ratio summary.",
                   saveCsv: true,
                   header: true
                );
                sb.AppendDescriptionParagraph($"Bivariate distributions statistics for MCR and cumulative exposure {individualDayUnits}, {individualDayUnits} are grouped by the highest contributing substance. The last column displays for each substance the number of {individualDayUnits} with cumulative exposure > 0 (n = {Model.DriverCompounds.Count}).");
                sb.AppendDescriptionParagraph($"Total number of {individualDayUnits} is {Model.DriverCompounds.Count}.");
                sb.AppendTable(
                   Model,
                   result,
                   "MCRSubstanceStatisticsTable",
                   ViewBag,
                   caption: "Maximum cumulative ratio statistics by substance.",
                   saveCsv: true,
                   header: true
                );
            } else {
                sb.AppendDescriptionParagraph($"The green line segments indicate the p95 percentiles in exposure ranges '< p5', 'p5 - p50', 'p50 - p95' and '> p95' (zero exposure {individualDayUnits} excluded).");
                if (Model.DriverCompounds.Count > 1) {
                    var chartCreator1 = new DriverCompoundsChartCreator(Model);
                    sb.AppendChart(
                            "DriverSubstancesChart",
                            chartCreator1,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator1.Title,
                            true
                        );
                    var chartCreator2 = new DriverCompoundsEllipsChartCreator(Model);
                    sb.AppendChart(
                            "DriverSubstancesEllipsChart",
                            chartCreator2,
                            ChartFileType.Svg,
                            Model,
                            ViewBag,
                            chartCreator2.Title,
                            true
                        );
                } else {
                    sb.AppendDescriptionParagraph("No MCR graph available (too few observations).");
                }
            }
        }
    }
}
