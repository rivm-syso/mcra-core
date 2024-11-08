using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
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

            var result = Model.DriverSubstanceTargetStatisticsRecords.OrderByDescending(c => c.CumulativeExposureMedian).ToList();
            //Render HTML
            var definition = "exposure";
            var riskType = string.Empty;
            if (Model.IsRiskMcrPlot) {
                definition = "risk";
                if (Model.RiskMetricType == RiskMetricType.HazardExposureRatio) {
                    riskType = "Risk is defined as hazard/exposure.";
                } else {
                    riskType = "Risk is defined as exposure/hazard.";
                }
            }
            var description = string.Empty;
            sb.AppendDescriptionParagraph($"Maximum Cumulative Ratio (MCR) plot: total {definition} / maximum {definition} vs total {definition} (n = {Model.DriverSubstanceTargets.Count}). {riskType}");
            if (!Model.IsRiskMcrPlot) {
                sb.AppendDescriptionParagraph($"Exposures are expressed in equivalents of the reference substance. For each {individualDayUnit} the {definition} is cumulated to a total {definition} and divided by the {definition} of the highest contributing substance (MCR).");
            } else {
                if (Model.RiskMetricCalculationType == RiskMetricCalculationType.RPFWeighted) {
                    sb.AppendDescriptionParagraph($"Exposures are expressed in equivalents of the reference substance. For each {individualDayUnit} the {definition} is cumulated to a total {definition} and divided by the {definition} of the highest contributing substance (MCR).");
                } else {
                    sb.AppendDescriptionParagraph($"For each {individualDayUnit} the {definition} is calculated as the sum of risk characterisation ratios and divided by the ratio of the highest contributing substance (MCR).");
                }
            }
            sb.AppendDescriptionParagraph($"Ratios above 1 indicate co-exposure, {individualDayUnits} have different colors according to the highest contributing substances.");

            sb.AppendDescriptionParagraph($"The black lines represent the regression lines MCR vs ln(Cumulative {definition}) for each tail.");
            if (Model.MinimumPercentage > 0) {
                sb.AppendDescriptionParagraph($"Substances with a contribution less than {Model.MinimumPercentage}% are not displayed.");
            }
            if (Model.IsRiskMcrPlot) {
                sb.AppendDescriptionParagraph($"The vertical red line indicates the toxicological threshold value.");
            }
            var panelBuilder = new HtmlTabPanelBuilder();

            if (Model.DriverSubstanceTargets.Count > 1) {
                if (!Model.SkipPrivacySensitiveOutputs) {
                    var chartCreator1 = new DriverSubstancesChartCreator(Model);
                    panelBuilder.AddPanel(
                        id: "totalChart",
                        title: "Scatter total distribution",
                        hoverText: "Scatter total distribution",
                        content: ChartHelpers.Chart(
                            name: "DriverSubstancesTotalChart",
                            section: Model,
                            viewBag: ViewBag,
                            chartCreator: chartCreator1,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true,
                            caption: chartCreator1.Title
                        )
                    );

                    var chartCreator2 = new DriverSubstancesChartCreator(Model, Model.Percentiles.Min());
                    panelBuilder.AddPanel(
                        id: "upperChart",
                        title: "Scatter upper distribution",
                        hoverText: "Scatter upper distribution",
                        content: ChartHelpers.Chart(
                            name: "DriverSubstancesUpperChart",
                            section: Model,
                            viewBag: ViewBag,
                            chartCreator: chartCreator2,
                            fileType: ChartFileType.Svg,
                            saveChartFile: true,
                            caption: chartCreator2.Title
                        )
                    );
                }
                var chartCreator3 = new DriverSubstancesEllipsChartCreator(Model, Model.SkipPrivacySensitiveOutputs);
                panelBuilder.AddPanel(
                    id: "totalEllipsChart",
                    title: "Ellipses total distribution",
                    hoverText: "Ellipses total distribution",
                    content: ChartHelpers.Chart(
                        name: "DriverSubstancesEllipseTotalChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator3,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: chartCreator3.Title
                    )
                );
                var chartCreator4 = new DriverSubstancesEllipsChartCreator(Model, Model.SkipPrivacySensitiveOutputs, Model.Percentiles.Min());
                panelBuilder.AddPanel(
                    id: "upperEllipsChart",
                    title: "Ellipses upper distribution",
                    hoverText: "Ellipses upper distribution",
                    content: ChartHelpers.Chart(
                        name: "DriverSubstancesEllipseUpperChart",
                        section: Model,
                        viewBag: ViewBag,
                        chartCreator: chartCreator4,
                        fileType: ChartFileType.Svg,
                        saveChartFile: true,
                        caption: chartCreator4.Title
                    )
                );
                panelBuilder.RenderPanel(sb);
            } else {
                sb.AppendDescriptionParagraph("No MCR graph available (too few observations).");
            }

            if (Model.MinimumPercentage > 0) {
                sb.AppendDescriptionParagraph($"Substances with a contribution less than {Model.MinimumPercentage}% are not displayed.");
            }
            if (Model.MCRDrilldownRecords?.Count > 0) {
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
            }

            if (!Model.IsRiskMcrPlot) {
                sb.AppendDescriptionParagraph($"Bivariate distributions statistics for MCR and cumulative {definition} {individualDayUnits}, {individualDayUnits} are grouped by the highest contributing substance. The last column displays for each substance the number of {individualDayUnits} with cumulative exposure > 0 (n = {Model.DriverSubstanceTargets.Count}).");
                sb.AppendDescriptionParagraph($"Total number of {individualDayUnits} is {Model.DriverSubstanceTargets.Count}.");
                sb.AppendTable(
                   Model,
                   result,
                   "MCRSubstanceStatisticsTable",
                   ViewBag,
                   caption: "Maximum cumulative ratio statistics by substance.",
                   saveCsv: true,
                   header: true
                );
            }
        }
    }
}
